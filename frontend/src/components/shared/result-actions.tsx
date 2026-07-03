"use client";

import { RotateCcw, X } from "lucide-react";
import { Button } from "@/components/ui/button";

interface ResultActionsProps {
  onClear: () => void;
  onRegenerate?: () => void;
  regenerateLabel?: string;
  clearing?: boolean;
}

export function ResultActions({
  onClear,
  onRegenerate,
  regenerateLabel = "Regenerate",
  clearing = false,
}: ResultActionsProps) {
  return (
    <div className="flex flex-wrap gap-3 mb-4">
      {onRegenerate && (
        <Button onClick={onRegenerate} disabled={clearing}>
          <RotateCcw className="h-4 w-4" />
          {regenerateLabel}
        </Button>
      )}
      <Button variant="secondary" onClick={onClear} disabled={clearing}>
        <X className="h-4 w-4" />
        Clear
      </Button>
    </div>
  );
}
