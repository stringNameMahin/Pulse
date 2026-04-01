import { useEffect, useState } from "react";
import Dashboard from "./pages/Dashboard";
import Processes from "./pages/Processes";
import Optimization from "./pages/Optimization";
import Settings from "./pages/Settings";
import Rules from "./pages/Rules";

function App() {
  const BASE_URL = "http://localhost:5196";

  const [isMobile, setIsMobile] = useState(window.innerWidth < 900);
  const [page, setPage] = useState("dashboard");

  const [status, setStatus] = useState(null);
  const [profile, setProfile] = useState("balanced");
  const [autoMode, setAutoMode] = useState(true);
  const [processes, setProcesses] = useState([]);

  const [theme, setTheme] = useState("light");
  const [priorityMode, setPriorityMode] = useState(false);

  const [priorityApps, setPriorityApps] = useState([]);
  const [newApp, setNewApp] = useState("");

  const [editingRule, setEditingRule] = useState(null);

  const [rules, setRules] = useState([]);
  const [newRule, setNewRule] = useState({
    triggerProcess: "",
    targetProfile: "balanced",
    closeApps: []
  });

  const isDark = theme === "dark";

  const fetchAll = async () => {
    try {
      const s = await fetch(`${BASE_URL}/status`).then(r => r.json());
      const p = await fetch(`${BASE_URL}/profiles`).then(r => r.json());
      const a = await fetch(`${BASE_URL}/auto-mode`).then(r => r.json());
      const pr = await fetch(`${BASE_URL}/processes`).then(r => r.json());
      const pm = await fetch(`${BASE_URL}/priority-mode`).then(r => r.json());
      const pa = await fetch(`${BASE_URL}/priority-apps`).then(r => r.json());
      const r = await fetch(`${BASE_URL}/rules`).then(r => r.json());

      setRules(r);
      setPriorityApps(pa);
      setStatus(s);
      setProfile(p.currentProfileId);
      setAutoMode(a.enabled);
      setProcesses(pr);
      setPriorityMode(pm.enabled);
    } catch (err) {
      console.error("[Pulse UI] Failed to fetch data", err);
    }
  };

  const deleteRule = async (id) => {
    await fetch(`${BASE_URL}/rules/${id}`, {
      method: "DELETE"
    });

    fetchAll();
  };

  const updateRule = async (id, updatedRule) => {
    const res = await fetch(`${BASE_URL}/rules/${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(updatedRule)
    });

    if (!res.ok) {
      alert("Duplicate or invalid rule");
      return;
    }

    setEditingRule(null);
    fetchAll();
  };

  const addRule = async () => {
    if (!newRule.triggerProcess.trim()) return;

    const trigger = newRule.triggerProcess.toLowerCase();

    const body = {
      triggerProcess: trigger,
      targetProfile: newRule.targetProfile,
      closeApps: newRule.closeApps || []
    };

    const res = await fetch(`${BASE_URL}/rules`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body)
    });

    if (!res.ok) {
      alert("Duplicate rule");
      return;
    }

    setNewRule({
      triggerProcess: "",
      targetProfile: "balanced",
      closeApps: []
    });

    fetchAll();
  };

  const toggleAutoMode = async () => {
    await fetch(`${BASE_URL}/auto-mode/${!autoMode ? "on" : "off"}`, {
      method: "POST"
    });

    setAutoMode(!autoMode);
  };

  const togglePriorityMode = async () => {
    await fetch(`${BASE_URL}/priority-mode/${!priorityMode ? "on" : "off"}`, {
      method: "POST"
    });

    setPriorityMode(!priorityMode);
  };

  const toggleTheme = () => {
    setTheme(isDark ? "light" : "dark");
  };

  const addPriorityApp = async () => {
    if (!newApp.trim()) return;

    const app = newApp.toLowerCase();

    if (priorityApps.includes(app)) {
      alert("App already added");
      return;
    }

    const updated = [...priorityApps, app];

    await fetch(`${BASE_URL}/priority-apps`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(updated)
    });

    setPriorityApps(updated);
    setNewApp("");
  };

  const removePriorityApp = async (app) => {
    const updated = priorityApps.filter(a => a !== app);

    await fetch(`${BASE_URL}/priority-apps`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(updated)
    });

    setPriorityApps(updated);
  };

  const switchProfile = async (id) => {
    const res = await fetch(`${BASE_URL}/apply-profile/${id}`, {
      method: "POST"
    });

    const data = await res.json();

    if (data.requiresAdmin) {
      const confirmRestart = window.confirm(
        "Pulse needs administrator access to apply system optimizations.\n\nRestart as admin?"
      );

      if (confirmRestart && window.electronAPI) {
        window.electronAPI.restartAsAdmin();
      }

      return;
    }

    setProfile(id);
    fetchAll();
  };

  useEffect(() => {
    fetchAll();

    const i = setInterval(() => {
      if (document.visibilityState === "visible") {
        fetchAll();
      }
    }, 2000);

    const handleResize = () => {
      setIsMobile(window.innerWidth < 900);
    };

    window.addEventListener("resize", handleResize);

    return () => {
      window.removeEventListener("resize", handleResize);
      clearInterval(i);
    };
  }, []);

  return (
    <div style={{
      display: "flex",
      flexDirection: isMobile ? "column" : "row",
      minHeight: "100vh",
      width: "100%",
      background: isDark ? "#1e1e1e" : "#eef2f7"
    }}>

      {/* SIDEBAR */}
      <div style={{
        width: isMobile ? "100%" : 220,
        display: isMobile ? "flex" : "block",
        justifyContent: isMobile ? "space-around" : "initial",
        background: isDark ? "#1f1f1f" : "#fff",
        padding: 20,
        borderRight: isDark ? "1px solid #333" : "1px solid #ddd"
      }}>
        <h2>Pulse</h2>

        <p onClick={() => setPage("dashboard")} style={{ cursor: "pointer", fontWeight: page === "dashboard" ? "bold" : "normal" }}>🏠 Dashboard</p>
        <p onClick={() => setPage("processes")} style={{ cursor: "pointer", fontWeight: page === "processes" ? "bold" : "normal" }}>📊 Processes</p>
        <p onClick={() => setPage("optimization")} style={{ cursor: "pointer", fontWeight: page === "optimization" ? "bold" : "normal" }}>⚙️ Optimization</p>
        <p onClick={() => setPage("rules")} style={{ cursor: "pointer", fontWeight: page === "rules" ? "bold" : "normal" }}>🧠 Rules</p>
        <p onClick={() => setPage("settings")} style={{ cursor: "pointer", fontWeight: page === "settings" ? "bold" : "normal" }}>🔧 Settings</p>
      </div>

      {/* MAIN */}
      <div style={{
        flex: 1,
        padding: 20,
        background: isDark ? "#1e1e1e" : "#eef2f7",
        color: isDark ? "#fff" : "#000"
      }}>
        {page === "dashboard" && <Dashboard status={status} isDark={isDark} isMobile={isMobile} />}
        {page === "processes" && <Processes processes={processes} isDark={isDark} />}
        {page === "optimization" && (
          <Optimization
            profile={profile}
            switchProfile={switchProfile}
            isDark={isDark}
          />
        )}
        {page === "settings" && (
          <Settings
            isDark={isDark}
            toggleTheme={toggleTheme}
            autoMode={autoMode}
            toggleAutoMode={toggleAutoMode}
            priorityMode={priorityMode}
            togglePriorityMode={togglePriorityMode}
            priorityApps={priorityApps}
            newApp={newApp}
            setNewApp={setNewApp}
            addPriorityApp={addPriorityApp}
            removePriorityApp={removePriorityApp}
          />
        )}
        {page === "rules" && (
          <Rules
            rules={rules}
            newRule={newRule}
            setNewRule={setNewRule}
            addRule={addRule}
            deleteRule={deleteRule}
            editingRule={editingRule}
            setEditingRule={setEditingRule}
            updateRule={updateRule}
            isDark={isDark}
          />
        )}
      </div>
    </div>
  );
}

export default App;