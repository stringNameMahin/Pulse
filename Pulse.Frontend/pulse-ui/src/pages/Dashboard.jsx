function Dashboard({ status, isDark, isMobile }) {

  const bg = isDark ? "#2c2c2c" : "#fff";
  const text = isDark ? "#fff" : "#000";

  const card = {
    background: bg,
    color: text,
    padding: 20,
    borderRadius: 12,
    boxShadow: "0 2px 10px rgba(0,0,0,0.1)",
    height: "100%"
  };

  const Circle = ({ value, label }) => {
    const angle = value * 3.6;

    return (
      <div style={{ textAlign: "center", color: text }}>
        <div style={{
          width: 90,
          height: 90,
          borderRadius: "50%",
          background: `conic-gradient(#4a90e2 ${angle}deg, #888 ${angle}deg)`,
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          margin: "auto"
        }}>
          <div style={{
            width: 65,
            height: 65,
            borderRadius: "50%",
            background: bg,
            display: "flex",
            alignItems: "center",
            justifyContent: "center"
          }}>
            {value}%
          </div>
        </div>
        <p style={{ marginTop: 8 }}>{label}</p>
      </div>
    );
  };

  const cpu = status?.cpuUsagePercent || 0;
  const ram = Math.round(
    (status?.ramUsedMB / status?.ramTotalMB) * 100 || 0
  );

  return (
    <div style={{ width: "100%" }}>
      <h2 style={{ textAlign: isMobile ? "center" : "left" }}>
        System Overview
      </h2>

      {/* GRID LAYOUT */}
      <div style={{
        display: "grid",
        gridTemplateColumns: isMobile ? "1fr" : "2fr 1fr",
        gap: 20,
        marginTop: 20,
        width: "100%",
        alignItems: "stretch"
      }}>

        {/* LEFT SIDE */}
        <div style={{
          display: "grid",
          gridTemplateColumns: isMobile ? "1fr" : "1fr 1fr",
          gap: 20,
          width: "100%",
          alignItems: "stretch"
        }}>
          
          <div style={card}>
            <Circle value={cpu} label="CPU Usage" />
          </div>

          <div style={card}>
            <Circle value={ram} label="Memory Usage" />
          </div>

        </div>

        {/* RIGHT SIDE */}
        <div style={{
          ...card,
          width: "100%",
          display: "flex",
          flexDirection: "column",
          justifyContent: "center",
          alignItems: "center",
          textAlign: "center"
        }}>
          <h3>System Status</h3>

          <p style={{
            color: cpu > 70 ? "red" : cpu > 40 ? "orange" : "green",
            fontWeight: "bold",
            marginTop: 10
          }}>
            {cpu > 70 ? "High Load" : cpu > 40 ? "Moderate" : "Optimal"}
          </p>

          <p style={{ marginTop: 10 }}>
            RAM: {status?.ramUsedMB} MB
          </p>
        </div>

      </div>
    </div>
  );
}

export default Dashboard;