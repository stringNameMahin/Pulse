import { useEffect, useState } from "react";
import Dashboard from "./pages/Dashboard";
import Processes from "./pages/Processes";
import Optimization from "./pages/Optimization";
import Settings from "./pages/Settings";
import Rules from "./pages/Rules";
import { useToast } from "./ToastContext";

function App() {
  const BASE_URL = "http://localhost:5126";
  const { addToast } = useToast();

  const [processes, setProcesses] = useState([]);
  const [editingRule, setEditingRule] = useState(null);
  const [page, setPage] = useState("dashboard");
  const [status, setStatus] = useState(null);
  const [profile, setProfile] = useState("balanced");
  const [autoMode, setAutoMode] = useState(true);
  const [priorityMode, setPriorityMode] = useState(false);
  const [priorityApps, setPriorityApps] = useState([]);
  const [rules, setRules] = useState([]);
  const [theme, setTheme] = useState("dark");
  const [newApp, setNewApp] = useState("");
  const [newRule, setNewRule] = useState({
    triggerProcess: "",
    targetProfile: "balanced",
    closeApps: []
  });

  const isDark = theme === "dark";

  // ----------------------
  // FETCH CORE DATA
  // ----------------------
  const fetchAll = async () => {
    try {
      const s = await fetch(`${BASE_URL}/status`).then(r => r.json());
      const p = await fetch(`${BASE_URL}/profiles`).then(r => r.json());
      const a = await fetch(`${BASE_URL}/auto-mode`).then(r => r.json());
      const pr = await fetch(`${BASE_URL}/processes`).then(r => r.json());
      const pm = await fetch(`${BASE_URL}/priority-mode`).then(r => r.json());
      const pa = await fetch(`${BASE_URL}/priority-apps`).then(r => r.json());
      const r = await fetch(`${BASE_URL}/rules`).then(r => r.json());

      setStatus(s);
      setProfile(p.currentProfileId);
      setAutoMode(a.enabled);
      setProcesses(pr);
      setPriorityMode(pm.enabled);
      setPriorityApps(pa);
      setRules(r);
    } catch {
      addToast("Backend fetch failed", "error");
    }
  };

  // ----------------------
  // EVENTS POLLING
  // ----------------------
  useEffect(() => {
    const interval = setInterval(async () => {
      try {
        const events = await fetch(`${BASE_URL}/events`).then(r => r.json());

        events.forEach(e => addToast(e, "info"));
      } catch { }
    }, 2000);

    return () => clearInterval(interval);
  }, []);

  useEffect(() => {
    const interval = setInterval(fetchAll, 3000);
    return () => clearInterval(interval);
  }, []);

  // ----------------------
  // INITIAL LOAD
  // ----------------------
  useEffect(() => {
    fetchAll();
  }, []);


  // ----------------------
  // ACTIONS
  // ----------------------

  const switchProfile = async (id) => {
    await fetch(`${BASE_URL}/apply-profile/${id}`, { method: "POST" });
    fetchAll();
  };

  const toggleAutoMode = async () => {
    const state = autoMode ? "off" : "on";
    await fetch(`${BASE_URL}/auto-mode/${state}`, { method: "POST" });
    fetchAll();
  };

  const togglePriorityMode = async () => {
    const state = priorityMode ? "off" : "on";
    await fetch(`${BASE_URL}/priority-mode/${state}`, { method: "POST" });
    fetchAll();
  };

  const addPriorityApp = async () => {
    if (!newApp.trim()) return;

    const updated = [...priorityApps, newApp];
    await fetch(`${BASE_URL}/priority-apps`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(updated)
    });

    setNewApp("");
    fetchAll();
  };

  const removePriorityApp = async (app) => {
    const updated = priorityApps.filter(a => a !== app);

    await fetch(`${BASE_URL}/priority-apps`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(updated)
    });

    fetchAll();
  };

  // Rules
  const addRule = async (newRule) => {
    await fetch(`${BASE_URL}/rules`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(newRule)
    });
    fetchAll();
  };

  const updateRule = async (id, updatedRule) => {
  await fetch(`${BASE_URL}/rules/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(updatedRule)
  });

  fetchAll();
};

  const deleteRule = async (id) => {
    await fetch(`${BASE_URL}/rules/${id}`, { method: "DELETE" });
    fetchAll();
  };

  return (
    <div style={{ display: "flex", height: "100vh" }}>

      {/* SIDEBAR */}
      <div style={{ width: 220, padding: 20 }}>
        <h2>⚡ Pulse</h2>

        {["dashboard", "processes", "optimization", "rules", "settings"].map(p => (
          <div key={p} onClick={() => setPage(p)} style={{ padding: 10, cursor: "pointer" }}>
            {p.toUpperCase()}
          </div>
        ))}
      </div>

      {/* MAIN */}
      <div style={{ flex: 1, padding: 20 }}>
        {page === "dashboard" && <Dashboard status={status} isDark={isDark} />}
        {page === "processes" && <Processes processes={processes} isDark={isDark} />}
        {page === "optimization" && <Optimization profile={profile} switchProfile={switchProfile} isDark={isDark} />}

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

        {page === "settings" && (
          <Settings
            isDark={isDark}
            toggleTheme={() => setTheme(isDark ? "light" : "dark")}
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
      </div>
    </div>
  );
}

export default App;