#!/usr/bin/env bash
# start-observer.sh — Background observer for continuous-learning-v2
# Analyzes observations.jsonl and detects patterns including token inefficiencies
# Usage: ./start-observer.sh [--once]

# Enable Bash strict mode: exit on command failures (-e), reject unset variables
# (-u), and fail a pipeline if any command in it fails (pipefail).
set -euo pipefail

# All observer state lives under ~/.copilot/homunculus so it can persist across
# repositories while still staying local to the current developer environment.
HOMUNCULUS_DIR="$HOME/.copilot/homunculus"

# Hooks append one JSON object per line to this file. Each line represents an
# observed prompt, tool call, or tool result from an agent session.
OBSERVATIONS_FILE="$HOMUNCULUS_DIR/observations.jsonl"

# Auto-detected learning hints are written as small Markdown files called
# "instincts." The personal directory is for patterns learned from this user.
INSTINCTS_DIR="$HOMUNCULUS_DIR/instincts/personal"

# This marker stores the number of observation lines analyzed last time. Keeping
# only a line count makes the observer cheap to resume without rewriting JSONL.
PROCESSED_MARKER="$HOMUNCULUS_DIR/.last_processed_line"

# Ensure the instinct output directory exists before analysis tries to create a
# new instinct file. The parent directories are created as needed by mkdir -p.
mkdir -p "$INSTINCTS_DIR"

# jq is required because each observation is stored as JSON. The script exits
# early with a clear message instead of failing later inside a pipeline.
if ! command -v jq &> /dev/null; then
  echo "Error: jq required for observation analysis"
  exit 1
fi

# Start at the beginning unless a previous run recorded how many JSONL lines it
# already handled. The value is intentionally global so analyze_observations can
# compare the current file length against the last completed analysis.
LAST_PROCESSED=0
if [ -f "$PROCESSED_MARKER" ]; then
  LAST_PROCESSED=$(cat "$PROCESSED_MARKER")
fi

analyze_observations() {
  local observations_file="$1"
  
  # It is valid for hooks to be configured before any events have been captured.
  # In that case there is simply nothing to process yet.
  if [ ! -f "$observations_file" ]; then
    echo "No observations to analyze"
    return
  fi
  
  # Count the full JSONL file and skip work if no new lines have appeared since
  # the previous observer run.
  local total_lines=$(wc -l < "$observations_file")
  if [ "$total_lines" -le "$LAST_PROCESSED" ]; then
    echo "No new observations since last analysis"
    return
  fi
  
  echo "Analyzing observations $((LAST_PROCESSED + 1)) to $total_lines..."
  
  # Extract only the unprocessed suffix of the observations file. All pattern
  # detection below intentionally works on the new batch, not on historical data.
  local new_obs=$(tail -n +$((LAST_PROCESSED + 1)) "$observations_file")
  
  # === TOKEN EFFICIENCY PATTERNS ===
  
  # Pattern 1: Repeated tool calls with the same tool name and serialized input.
  # This catches cases where the agent spent tokens making an identical request
  # more than once instead of reusing already available context.
  echo ""
  echo "=== Token Efficiency Analysis ==="
  
  # jq -s slurps all new JSONL rows into an array. The query keeps only the tool
  # name and a stringified input, groups identical pairs, then counts groups with
  # more than one member. Malformed or unexpected JSON falls back to 0.
  local repeated=$(echo "$new_obs" | jq -s '
    [.[] | {tool, input: (.input | tostring)}] |
    group_by(.tool + .input) |
    map(select(length > 1)) | length
  ' 2>/dev/null || echo "0")
  
  if [ "$repeated" -gt 0 ]; then
    echo "⚠️  Repeated identical tool calls: $repeated (token waste)"
  fi
  
  # Pattern 2: Sequential read/search operations. Adjacent view/grep calls are a
  # signal that independent context gathering might have been batched in parallel
  # to save round trips and reduce conversation churn.
  local seq_reads=$(echo "$new_obs" | jq -s '
    [range(length - 1) as $i | 
     if (.[$i].tool == "view" or .[$i].tool == "grep") and 
        (.[$i + 1].tool == "view" or .[$i + 1].tool == "grep")
     then 1 else 0 end] | add // 0
  ' 2>/dev/null || echo "0")
  
  if [ "$seq_reads" -gt 2 ]; then
    echo "💡 Sequential read operations: $seq_reads (consider parallel calls)"
  fi
  
  # Pattern 3: Verbose bash commands. Commands that lack quiet flags, pager
  # suppression, or head/tail filtering can flood the transcript with output and
  # waste tokens.
  local verbose_count=$(echo "$new_obs" | jq -r '
    select(.tool == "bash") |
    select(.input.command != null) |
    select(.input.command | test("--quiet|--silent|\\| head|\\| tail|--no-pager|-q") | not) |
    .input.command
  ' 2>/dev/null | wc -l || echo "0")
  
  if [ "$verbose_count" -gt 3 ]; then
    echo "💡 Verbose commands without output limiting: $verbose_count"
  fi
  
  # Pattern 4: Failed operations. Post-tool observations include results, so this
  # query counts explicit errors or nonzero exit codes that may point to better
  # future workflows.
  local failures=$(echo "$new_obs" | jq -s '
    [.[] | select(.type == "post") | 
     select(.result.error != null or .result.exitCode != null and .result.exitCode != 0)] | length
  ' 2>/dev/null || echo "0")
  
  if [ "$failures" -gt 0 ]; then
    echo "📊 Failed operations: $failures (review for better approaches)"
  fi
  
  # === WORKFLOW PATTERNS ===
  
  # Print the five most common tools in the new observation batch. This is a
  # lightweight status summary for humans running the observer manually.
  echo ""
  echo "=== Tool Usage Summary ==="
  echo "$new_obs" | jq -r '.tool' 2>/dev/null | sort | uniq -c | sort -rn | head -5
  
  # === GENERATE EFFICIENCY INSTINCT IF PATTERNS FOUND ===
  
  # If the batch shows enough redundant or sequential calls, create a reusable
  # instinct. Existing instincts are left untouched so the observer does not
  # overwrite user-edited notes or repeatedly churn the same file.
  if [ "$repeated" -gt 2 ] || [ "$seq_reads" -gt 3 ]; then
    local instinct_file="$INSTINCTS_DIR/reduce-redundant-calls.md"
    if [ ! -f "$instinct_file" ]; then
      # The heredoc is single-quoted to prevent shell interpolation inside the
      # Markdown frontmatter/body. This keeps the generated instinct deterministic.
      cat > "$instinct_file" << 'EOF'
---
id: reduce-redundant-calls
trigger: "when making multiple similar tool calls"
confidence: 0.6
domain: "token-efficiency"
source: "observer-detected"
---

# Reduce Redundant Tool Calls

## Action
Before making a tool call, check if you already have the needed information.
Use parallel tool calls when operations are independent.

## Evidence
- Observer detected repeated identical tool calls
- Sequential read operations that could be parallelized
EOF
      echo "✨ Created instinct: reduce-redundant-calls.md"
    fi
  fi
  
  # Record progress only after analysis and any instinct generation completes.
  # A later run will resume from the next line.
  echo "$total_lines" > "$PROCESSED_MARKER"
  echo ""
  echo "✅ Analyzed $((total_lines - LAST_PROCESSED)) new observations"
}

# Main entry point. With --once, the script performs a single analysis pass for
# manual checks or tests. Without arguments, it loops forever and sleeps between
# passes so it can act as a low-overhead background observer.
if [ "${1:-}" = "--once" ]; then
  analyze_observations "$OBSERVATIONS_FILE"
else
  echo "Starting continuous observer (Ctrl+C to stop)..."
  echo "Checking every 5 minutes for new observations..."
  while true; do
    analyze_observations "$OBSERVATIONS_FILE"
    # Sleep for five minutes between checks. The observer does not need real-time
    # precision because it analyzes append-only observations in batches.
    sleep 300
  done
fi
