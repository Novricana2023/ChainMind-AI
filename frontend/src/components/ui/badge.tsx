import * as React from "react";
import { cva, type VariantProps } from "class-variance-authority";
import { cn } from "@/lib/utils";

const badgeVariants = cva(
  "inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium",
  {
    variants: {
      variant: {
        default: "bg-purple-500/20 text-purple-300 border border-purple-500/30",
        critical: "bg-red-500/20 text-red-300 border border-red-500/30",
        high: "bg-orange-500/20 text-orange-300 border border-orange-500/30",
        medium: "bg-yellow-500/20 text-yellow-300 border border-yellow-500/30",
        low: "bg-blue-500/20 text-blue-300 border border-blue-500/30",
        info: "bg-gray-500/20 text-gray-300 border border-gray-500/30",
        success: "bg-emerald-500/20 text-emerald-300 border border-emerald-500/30",
      },
    },
    defaultVariants: { variant: "default" },
  }
);

export interface BadgeProps
  extends React.HTMLAttributes<HTMLDivElement>,
    VariantProps<typeof badgeVariants> {}

export function Badge({ className, variant, ...props }: BadgeProps) {
  return <div className={cn(badgeVariants({ variant }), className)} {...props} />;
}

export function severityVariant(severity: string): VariantProps<typeof badgeVariants>["variant"] {
  const s = severity.toLowerCase();
  if (s === "critical") return "critical";
  if (s === "high") return "high";
  if (s === "medium") return "medium";
  if (s === "low") return "low";
  return "info";
}
