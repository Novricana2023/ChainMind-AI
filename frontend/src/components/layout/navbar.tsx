"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { motion } from "framer-motion";
import {
  Brain,
  LayoutDashboard,
  Code2,
  BookOpen,
  Shield,
  Rocket,
  FlaskConical,
  Fuel,
  Bot,
  FileText,
  Menu,
  X,
} from "lucide-react";
import { useState } from "react";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";

const navItems = [
  { href: "/dashboard", label: "Dashboard", icon: LayoutDashboard },
  { href: "/app/generator", label: "Generator", icon: Code2 },
  { href: "/app/explainer", label: "Explainer", icon: BookOpen },
  { href: "/app/auditor", label: "Auditor", icon: Shield },
  { href: "/app/deploy", label: "Deploy", icon: Rocket },
  { href: "/app/tests", label: "Tests", icon: FlaskConical },
  { href: "/app/gas", label: "Gas", icon: Fuel },
  { href: "/app/agent", label: "Agent", icon: Bot },
  { href: "/app/docs", label: "Docs", icon: FileText },
];

export function Navbar() {
  const pathname = usePathname();
  const isLanding = pathname === "/";
  const [mobileOpen, setMobileOpen] = useState(false);

  return (
    <header className="fixed top-0 left-0 right-0 z-50 border-b border-white/5 bg-black/60 backdrop-blur-xl">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex items-center justify-between h-16">
          <Link href="/" className="flex items-center gap-2 group">
            <div className="p-1.5 rounded-lg bg-gradient-to-br from-purple-600 to-cyan-600">
              <Brain className="h-5 w-5 text-white" />
            </div>
            <span className="font-bold text-lg text-white group-hover:text-purple-300 transition-colors">
              ChainMind<span className="text-purple-400">AI</span>
            </span>
          </Link>

          <nav className="hidden md:flex items-center gap-1">
            {isLanding ? (
              <a href="#features" className="px-3 py-2 text-sm text-gray-400 hover:text-white transition-colors">Features</a>
            ) : (
              navItems.slice(0, 5).map((item) => (
                <Link
                  key={item.href}
                  href={item.href}
                  className={cn(
                    "px-3 py-2 text-sm rounded-lg transition-colors",
                    pathname === item.href
                      ? "text-purple-300 bg-purple-500/10"
                      : "text-gray-400 hover:text-white hover:bg-white/5"
                  )}
                >
                  {item.label}
                </Link>
              ))
            )}
          </nav>

          <div className="flex items-center gap-3">
            <Link href="/dashboard" className="hidden sm:block">
              <Button variant="secondary" size="sm">Dashboard</Button>
            </Link>
            <Link href="/app/generator">
              <Button size="sm">Get Started</Button>
            </Link>
            <button
              className="md:hidden p-2 text-gray-400 hover:text-white"
              onClick={() => setMobileOpen(!mobileOpen)}
            >
              {mobileOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
            </button>
          </div>
        </div>
      </div>

      {mobileOpen && (
        <motion.div
          initial={{ opacity: 0, y: -10 }}
          animate={{ opacity: 1, y: 0 }}
          className="md:hidden border-t border-white/5 bg-black/90 backdrop-blur-xl"
        >
          <div className="px-4 py-4 space-y-1">
            {(isLanding
              ? [{ href: "#features", label: "Features" }]
              : navItems
            ).map((item) => (
              <Link
                key={item.href}
                href={item.href}
                className="block px-3 py-2 text-sm text-gray-400 hover:text-white rounded-lg hover:bg-white/5"
                onClick={() => setMobileOpen(false)}
              >
                {item.label}
              </Link>
            ))}
          </div>
        </motion.div>
      )}
    </header>
  );
}

export function Sidebar() {
  const pathname = usePathname();

  return (
    <aside className="hidden lg:flex flex-col w-64 border-r border-white/5 bg-black/40 backdrop-blur-xl min-h-[calc(100vh-4rem)] p-4">
      <p className="text-xs font-medium text-gray-500 uppercase tracking-wider mb-4 px-3">Tools</p>
      <nav className="space-y-1">
        {navItems.map((item) => {
          const Icon = item.icon;
          const active = pathname === item.href;
          return (
            <Link
              key={item.href}
              href={item.href}
              className={cn(
                "flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm transition-all",
                active
                  ? "bg-gradient-to-r from-purple-600/20 to-cyan-600/20 text-white border border-purple-500/20"
                  : "text-gray-400 hover:text-white hover:bg-white/5"
              )}
            >
              <Icon className={cn("h-4 w-4", active && "text-purple-400")} />
              {item.label}
            </Link>
          );
        })}
      </nav>
    </aside>
  );
}
