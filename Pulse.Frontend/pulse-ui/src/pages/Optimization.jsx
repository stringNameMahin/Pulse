function Optimization({ profile, switchProfile, isDark }) {

  const bg = isDark ? "#2c2c2c" : "#fff";
  const text = isDark ? "#fff" : "#000";

  const card = {
    background: bg,
    color: text,
    padding: 20,
    borderRadius: 12,
    boxShadow: "0 2px 10px rgba(0,0,0,0.1)",
    width: "100%"
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
      <h2>Optimization</h2>

      <div style={{ ...card, marginTop: 20 }}>

        <div style={{
          display: "flex",
          flexDirection: "column",
          gap: 10
        }}>
          
          <button
            onClick={() => switchProfile("balanced")}
            style={buttonStyle(profile === "balanced")}
          >
            Balanced Mode
          </button>

          <button
            onClick={() => switchProfile("high")}
            style={buttonStyle(profile === "high")}
          >
            High Performance
          </button>

        </div>

        <p style={{ marginTop: 15 }}>
          Current Profile: <b>{profile}</b>
        </p>

      </div>
    </div>
  );
}

export default Optimization;