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

  const bg = isDark ? "#2c2c2c" : "#fff";
  const text = isDark ? "#fff" : "#000";

  const card = {
    background: bg,
    color: text,
    padding: 20,
    borderRadius: 12,
    boxShadow: "0 2px 10px rgba(0,0,0,0.1)",
    maxWidth: "100%"
  };

  const buttonStyle = (active) => ({
    padding: "10px 15px",
    borderRadius: 8,
    border: "none",
    cursor: "pointer",
    width: "100%",
    background: active ? "#4a90e2" : (isDark ? "#444" : "#eee"),
    color: active ? "#fff" : text,
    fontWeight: active ? "bold" : "normal"
  });

  return (
    <div>
      <h2>Settings</h2>

      <div style={{ ...card, marginTop: 20 }}>

        {/* CONTROLS */}
        <div style={{
          display: "flex",
          flexDirection: "column",
          gap: 10
        }}>

          <button
            onClick={toggleTheme}
            style={buttonStyle(isDark)}
          >
            Theme: {isDark ? "Dark" : "Light"}
          </button>

          <button
            onClick={toggleAutoMode}
            style={buttonStyle(autoMode)}
          >
            Auto Mode: {autoMode ? "ON" : "OFF"}
          </button>

          <button
            onClick={togglePriorityMode}
            style={buttonStyle(priorityMode)}
          >
            Smart Priority: {priorityMode ? "ON" : "OFF"}
          </button>
          <div style={{ marginTop: 20 }}>
            <h3>Priority Apps</h3>

            {/* INPUT */}
            <div style={{ display: "flex", gap: 10, marginTop: 10 }}>
              <input
                placeholder="Enter app name (e.g. chrome)"
                value={newApp}
                onChange={(e) => setNewApp(e.target.value)}
                style={{
                  flex: 1,
                  padding: 8,
                  borderRadius: 6,
                  border: "1px solid #ccc",
                  background: isDark ? "#444" : "#fff",
                  color: isDark ? "#fff" : "#000"
                }}
              />

              <button
                onClick={addPriorityApp}
                style={{
                  padding: "8px 12px",
                  borderRadius: 6,
                  border: "none",
                  cursor: "pointer"
                }}
              >
                Add
              </button>
            </div>

            {/* LIST */}
            <div style={{ marginTop: 15, display: "flex", flexDirection: "column", gap: 8 }}>
              {priorityApps.map(app => (
                <div
                  key={app}
                  style={{
                    display: "flex",
                    justifyContent: "space-between",
                    alignItems: "center",
                    padding: 8,
                    borderRadius: 6,
                    background: isDark ? "#444" : "#eee"
                  }}
                >
                  <span>{app}</span>

                  <button
                    onClick={() => removePriorityApp(app)}
                    style={{
                      border: "none",
                      background: "red",
                      color: "#fff",
                      borderRadius: 6,
                      padding: "4px 8px",
                      cursor: "pointer"
                    }}
                  >
                    X
                  </button>
                </div>
              ))}
            </div>
          </div>

        </div>

      </div>
    </div>
  );
}

export default Settings;