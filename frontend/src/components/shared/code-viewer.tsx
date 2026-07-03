"use client";

import { Prism as SyntaxHighlighter } from "react-syntax-highlighter";
import { oneDark } from "react-syntax-highlighter/dist/esm/styles/prism";
import { Copy, Download, Check } from "lucide-react";
import { useState } from "react";
import { Button } from "@/components/ui/button";

interface CodeViewerProps {
  code: string;
  language?: string;
  filename?: string;
}

export function CodeViewer({ code, language = "solidity", filename }: CodeViewerProps) {
  const [copied, setCopied] = useState(false);

  const handleCopy = async () => {
    await navigator.clipboard.writeText(code);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const handleDownload = () => {
    const ext = language === "javascript" ? "js" : language === "markdown" ? "md" : "sol";
    const blob = new Blob([code], { type: "text/plain" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = filename || `contract.${ext}`;
    a.click();
    URL.revokeObjectURL(url);
  };

  return (
    <div className="relative rounded-xl border border-white/10 overflow-hidden">
      <div className="flex items-center justify-between px-4 py-2 bg-black/60 border-b border-white/10">
        <span className="text-xs text-gray-400 font-mono">{filename || language}</span>
        <div className="flex gap-2">
          <Button variant="ghost" size="sm" onClick={handleCopy}>
            {copied ? <Check className="h-4 w-4 text-emerald-400" /> : <Copy className="h-4 w-4" />}
            {copied ? "Copied" : "Copy"}
          </Button>
          <Button variant="ghost" size="sm" onClick={handleDownload}>
            <Download className="h-4 w-4" />
            Download
          </Button>
        </div>
      </div>
      <SyntaxHighlighter
        language={language}
        style={oneDark}
        customStyle={{
          margin: 0,
          padding: "1rem",
          background: "rgba(0,0,0,0.4)",
          fontSize: "0.8125rem",
        }}
      >
        {code}
      </SyntaxHighlighter>
    </div>
  );
}
