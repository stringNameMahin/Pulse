import { getColors } from "../theme";

function Settings({
  isDark,
  toggleTheme,
  autoMode,
  toggleAutoMode,
  priorityMode,
  togglePriorityMode,
  priorityApps,
  newApp,
  setNewApp,
  addPriorityApp,
  removePriorityApp
}) {

  const colors = getColors(isDark);

  return (
    <div style={{ color: colors.text }}>

      <h2>🔧 Settings</h2>
      <p style={{ color: colors.subtext }}>
        Customize Pulse behavior and preferences
      </p>

      <div style={{
        display: "grid",
        gridTemplateColumns: "repeat(auto-fit, minmax(280px, 1fr))",
        gap: 20,
        marginTop: 20
      }}>

        {/* GENERAL */}
        <div style={section(colors)}>
          <h3>General</h3>

          <div style={{ display: "flex", flexDirection: "column", gap: 10 }}>
            <button onClick={toggleTheme} style={toggleBtn(colors, isDark)}>
              Theme: {isDark ? "Dark" : "Light"}
            </button>

            <button onClick={toggleAutoMode} style={toggleBtn(colors, autoMode)}>
              Auto Mode: {autoMode ? "ON" : "OFF"}
            </button>

            <button onClick={togglePriorityMode} style={toggleBtn(colors, priorityMode)}>
              Smart Priority: {priorityMode ? "ON" : "OFF"}
            </button>
          </div>
        </div>

        {/* PRIORITY APPS */}
        <div style={section(colors)}>
          <h3>Priority Apps</h3>

          <div style={{ display: "flex", gap: 10, marginTop: 10 }}>
            <input
              placeholder="Enter app name"
              value={newApp}
              onChange={(e) => setNewApp(e.target.value)}
              style={input(colors)}
            />

            <button onClick={addPriorityApp} style={primaryBtn(colors)}>
              Add
            </button>
          </div>

          <div style={{ marginTop: 15 }}>
            {priorityApps.map(app => (
              <div key={app} style={listItem(colors)}>
                <span>{app}</span>

                <button onClick={() => removePriorityApp(app)} style={deleteBtn(colors)}>
                  ✕
                </button>
              </div>
            ))}
          </div>
        </div>

      </div>
    </div>
  );
}

/* STYLES */

const section = (c) => ({
  background: c.card,
  padding: 20,
  borderRadius: 16
});

const toggleBtn = (c, active) => ({
  padding: "10px",
  borderRadius: 10,
  border: "none",
  background: active ? c.accent : c.hover,
  color: active ? "#fff" : c.text,
  cursor: "pointer"
});

const input = (c) => ({
  flex: 1,
  padding: 10,
  borderRadius: 10,
  border: "none",
  background: c.bg,
  color: c.text
});

const primaryBtn = (c) => ({
  padding: "10px",
  borderRadius: 10,
  border: "none",
  background: c.accent,
  color: "#fff",
  cursor: "pointer"
});

const listItem = (c) => ({
  display: "flex",
  justifyContent: "space-between",
  padding: 10,
  borderRadius: 10,
  background: c.bg,
  marginTop: 5
});

const deleteBtn = (c) => ({
  background: c.danger,
  color: "#fff",
  border: "none",
  borderRadius: 6,
  cursor: "pointer"
});

export default Settings;