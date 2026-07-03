"use client";

import { Sidebar } from "@/components/layout/navbar";

interface AppLayoutProps {
  title: string;
  children: React.ReactNode;
}

export function AppLayout({ title, children }: AppLayoutProps) {
  return (
    <div className="flex pt-16 min-h-screen">
      <Sidebar />
      <main className="flex-1 p-6 lg:p-8 max-w-5xl">
        <h1 className="text-2xl lg:text-3xl font-bold text-white mb-8">{title}</h1>
        {children}
      </main>
    </div>
  );
}
