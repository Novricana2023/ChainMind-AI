"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { motion } from "framer-motion";
import {
  Code2,
  Shield,
  TrendingUp,
  Clock,
  ArrowRight,
  Layers,
} from "lucide-react";
import { api } from "@/lib/api";
import { formatDate } from "@/lib/utils";
import type { DashboardStats, ProjectItem, TemplateItem, SessionItem } from "@/types";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { LoadingState } from "@/components/shared/loading-state";

export default function DashboardPage() {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [projects, setProjects] = useState<ProjectItem[]>([]);
  const [templates, setTemplates] = useState<TemplateItem[]>([]);
  const [sessions, setSessions] = useState<SessionItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    Promise.all([
      api.getStats(),
      api.getProjects(),
      api.getTemplates(),
      api.getSessions(),
    ])
      .then(([s, p, t, sess]) => {
        setStats(s);
        setProjects(p);
        setTemplates(t);
        setSessions(sess);
      })
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <div className="pt-16"><LoadingState message="Loading dashboard..." /></div>;

  const statCards = stats
    ? [
        { label: "Contracts Generated", value: stats.contractsGenerated, icon: Code2, color: "text-purple-400" },
        { label: "Audits Completed", value: stats.auditsCompleted, icon: Shield, color: "text-cyan-400" },
        { label: "Security Score Avg", value: `${stats.securityScoreAverage}%`, icon: TrendingUp, color: "text-emerald-400" },
        { label: "AI Sessions", value: stats.activeSessions, icon: Clock, color: "text-orange-400" },
      ]
    : [];

  return (
    <div className="pt-16 min-h-screen">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <h1 className="text-3xl font-bold mb-8">Dashboard</h1>

        {error && (
          <div className="mb-6 p-4 rounded-lg bg-red-500/10 border border-red-500/20 text-red-300 text-sm">
            {error} — Make sure the backend API is running.
          </div>
        )}

        <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
          {statCards.map((s, i) => (
            <motion.div
              key={s.label}
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: i * 0.1 }}
            >
              <Card>
                <CardContent className="pt-6">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-sm text-gray-400">{s.label}</p>
                      <p className="text-2xl font-bold mt-1">{s.value}</p>
                    </div>
                    <s.icon className={`h-8 w-8 ${s.color} opacity-80`} />
                  </div>
                </CardContent>
              </Card>
            </motion.div>
          ))}
        </div>

        <div className="grid lg:grid-cols-2 gap-6 mb-8">
          <Card>
            <CardHeader>
              <CardTitle>Recent Projects</CardTitle>
            </CardHeader>
            <CardContent>
              {projects.length === 0 ? (
                <p className="text-gray-500 text-sm">No projects yet. Generate your first contract!</p>
              ) : (
                <div className="space-y-3">
                  {projects.map((p) => (
                    <div key={p.id} className="flex items-center justify-between p-3 rounded-lg bg-white/5">
                      <div>
                        <p className="font-medium text-sm">{p.name}</p>
                        <p className="text-xs text-gray-500">{formatDate(p.createdAt)}</p>
                      </div>
                      <Badge variant={p.status === "Complete" ? "success" : "default"}>{p.status}</Badge>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Sessions</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {sessions.map((s) => (
                  <div key={s.id} className="flex items-center justify-between p-3 rounded-lg bg-white/5">
                    <div>
                      <p className="font-medium text-sm">{s.title}</p>
                      <p className="text-xs text-gray-500">{formatDate(s.createdAt)}</p>
                    </div>
                    <Badge>{s.type}</Badge>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </div>

        <Card className="mb-8">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Layers className="h-5 w-5 text-purple-400" />
              Templates
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
              {templates.map((t) => (
                <Link key={t.id} href="/app/generator">
                  <div className="p-4 rounded-lg border border-white/10 hover:border-purple-500/30 transition-colors cursor-pointer">
                    <Badge className="mb-2">{t.category}</Badge>
                    <p className="font-medium text-sm">{t.name}</p>
                    <p className="text-xs text-gray-500 mt-1">{t.description}</p>
                  </div>
                </Link>
              ))}
            </div>
          </CardContent>
        </Card>

        <div className="flex gap-4">
          <Link href="/app/generator">
            <Button>Generate Contract <ArrowRight className="h-4 w-4" /></Button>
          </Link>
          <Link href="/app/auditor">
            <Button variant="secondary">Run Audit</Button>
          </Link>
        </div>
      </div>
    </div>
  );
}
