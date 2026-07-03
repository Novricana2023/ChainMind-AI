"use client";

import { Loader2 } from "lucide-react";

interface LoadingStateProps {
  message?: string;
}

export function LoadingState({ message = "Processing..." }: LoadingStateProps) {
  return (
    <div className="flex flex-col items-center justify-center py-16 gap-4">
      <div className="relative">
        <div className="absolute inset-0 rounded-full bg-purple-500/20 blur-xl animate-pulse" />
        <Loader2 className="h-10 w-10 text-purple-400 animate-spin relative" />
      </div>
      <p className="text-gray-400 text-sm">{message}</p>
    </div>
  );
}

export function EmptyState({ title, description }: { title: string; description: string }) {
  return (
    <div className="flex flex-col items-center justify-center py-16 text-center">
      <div className="w-16 h-16 rounded-2xl bg-purple-500/10 border border-purple-500/20 flex items-center justify-center mb-4">
        <span className="text-2xl">⛓️</span>
      </div>
      <h3 className="text-lg font-medium text-white mb-2">{title}</h3>
      <p className="text-gray-400 text-sm max-w-md">{description}</p>
    </div>
  );
}
