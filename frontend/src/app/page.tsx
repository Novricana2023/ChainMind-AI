"use client";

import Link from "next/link";
import { motion } from "framer-motion";
import {
  Code2,
  BookOpen,
  Shield,
  Rocket,
  FlaskConical,
  Fuel,
  Bot,
  FileText,
  ArrowRight,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

const features = [
  { icon: Code2, title: "Generator", href: "/app/generator" },
  { icon: BookOpen, title: "Explainer", href: "/app/explainer" },
  { icon: Shield, title: "Auditor", href: "/app/auditor" },
  { icon: Rocket, title: "Deploy", href: "/app/deploy" },
  { icon: FlaskConical, title: "Tests", href: "/app/tests" },
  { icon: Fuel, title: "Gas", href: "/app/gas" },
  { icon: Bot, title: "Agent", href: "/app/agent" },
  { icon: FileText, title: "Docs", href: "/app/docs" },
];

const fadeUp = {
  initial: { opacity: 0, y: 20 },
  whileInView: { opacity: 1, y: 0 },
  viewport: { once: true },
  transition: { duration: 0.5 },
};

export default function LandingPage() {
  return (
    <div className="pt-16">
      <section className="relative hero-glow grid-bg overflow-hidden">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-24 lg:py-32">
          <motion.div
            initial={{ opacity: 0, y: 30 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.7 }}
            className="text-center max-w-4xl mx-auto"
          >
            <h1 className="text-4xl sm:text-5xl lg:text-7xl font-bold tracking-tight mb-6">
              Build Smarter Products{" "}
              <span className="gradient-text">with AI</span>
            </h1>
            <p className="text-lg sm:text-xl text-gray-400 mb-10 max-w-2xl mx-auto">
              Generate, explain, audit, and optimize smart contracts.
            </p>
            <div className="flex flex-col sm:flex-row items-center justify-center gap-4">
              <Link href="/app/generator">
                <Button size="lg" className="w-full sm:w-auto">
                  Get Started <ArrowRight className="h-4 w-4" />
                </Button>
              </Link>
              <Link href="/dashboard">
                <Button variant="secondary" size="lg" className="w-full sm:w-auto">
                  Dashboard
                </Button>
              </Link>
            </div>
          </motion.div>

          <motion.div
            initial={{ opacity: 0, y: 40 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.7, delay: 0.3 }}
            className="mt-16 max-w-4xl mx-auto"
          >
            <div className="glass-card rounded-2xl p-1 shadow-2xl shadow-purple-500/10">
              <div className="rounded-xl bg-black/80 p-6 font-mono text-sm">
                <div className="flex items-center gap-2 mb-4">
                  <div className="w-3 h-3 rounded-full bg-red-500/80" />
                  <div className="w-3 h-3 rounded-full bg-yellow-500/80" />
                  <div className="w-3 h-3 rounded-full bg-green-500/80" />
                  <span className="ml-2 text-gray-500 text-xs">HealingToken.sol</span>
                </div>
                <pre className="text-gray-300 overflow-x-auto">
                  <code>{`contract HealingToken is ERC20, Ownable {
  constructor() ERC20("HealingToken", "HEAL") {
    _mint(msg.sender, 1_000_000 * 10**decimals());
  }
}`}</code>
                </pre>
              </div>
            </div>
          </motion.div>
        </div>
      </section>

      <section id="features" className="py-24 border-t border-white/5">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <motion.div {...fadeUp} className="text-center mb-16">
            <h2 className="text-3xl lg:text-4xl font-bold mb-4">Features</h2>
          </motion.div>
          <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-6">
            {features.map((f, i) => (
              <motion.div key={f.title} {...fadeUp} transition={{ delay: i * 0.05 }}>
                <Link href={f.href}>
                  <Card className="h-full hover:border-purple-500/30 transition-colors group cursor-pointer">
                    <CardHeader className="flex flex-row items-center gap-3 space-y-0">
                      <div className="p-2.5 rounded-lg bg-purple-500/10 group-hover:bg-purple-500/20 transition-colors">
                        <f.icon className="h-5 w-5 text-purple-400" />
                      </div>
                      <CardTitle className="text-base">{f.title}</CardTitle>
                    </CardHeader>
                  </Card>
                </Link>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      <footer className="border-t border-white/5 py-8">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 flex flex-col sm:flex-row items-center justify-between gap-4">
          <p className="text-sm text-gray-500">© 2026 ChainMind AI</p>
          <div className="flex gap-6 text-sm text-gray-500">
            <Link href="/app/generator" className="hover:text-white transition-colors">Generator</Link>
            <Link href="/app/auditor" className="hover:text-white transition-colors">Auditor</Link>
            <Link href="/dashboard" className="hover:text-white transition-colors">Dashboard</Link>
          </div>
        </div>
      </footer>
    </div>
  );
}
