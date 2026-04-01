import { getColors } from "../theme";
function Dashboard({ status, isDark, isMobile }) {


  const colors = getColors(isDark);

  const cpu = status?.cpuUsagePercent ?? 0;
const ram = status?.ramTotalMB
  ? Math.round((status.ramUsedMB / status.ramTotalMB) * 100)
  : 0;

  const getStatus = () => {
    if (cpu > 70) return { label: "High Load", color: "#ef4444" };
    if (cpu > 40) return { label: "Moderate", color: "#f59e0b" };
    return { label: "Optimal", color: "#22c55e" };
  };

  const statusInfo = getStatus();

  const Circle = ({ value, label }) => {
    const angle = value * 3.6;

    return (
      <div style={{ textAlign: "center" }}>
        <div style={{
          width: 110,
          height: 110,
          borderRadius: "50%",
          background: `conic-gradient(${colors.accent} ${angle}deg, ${colors.bg} ${angle}deg)`,
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          margin: "auto"
        }}>
          <div style={{
            width: 80,
            height: 80,
            borderRadius: "50%",
            background: colors.bg,
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            fontWeight: "bold",
            color: colors.text
          }}>
            {value}%
          </div>
        </div>
        <p style={{ marginTop: 10, color: colors.subtext }}>{label}</p>
      </div>
    );
  };

  const card = {
    background: colors.card,
    color: colors.text,
    padding: 20,
    borderRadius: 16,
    boxShadow: "0 10px 25px rgba(0,0,0,0.2)",
    transition: "0.2s"
  };

  return (
    <div>
      <h2>📊 System Overview</h2>
      <p style={{ color: colors.subtext }}>Real-time performance insights</p>

      <div style={{
        display: "grid",
        gridTemplateColumns: isMobile ? "1fr" : "2fr 1fr",
        gap: 20,
        marginTop: 20
      }}>

        <div style={{
          display: "grid",
          gridTemplateColumns: isMobile ? "1fr" : "1fr 1fr",
          gap: 20
        }}>
          {[{ v: cpu, l: "CPU" }, { v: ram, l: "Memory" }].map((item, i) => (
            <div key={i}
              style={card}
              onMouseOver={e => e.currentTarget.style.transform = "translateY(-4px) scale(1.01)"}
              onMouseOut={e => e.currentTarget.style.transform = "translateY(0px) scale(1)"}
            >
              <Circle value={item.v} label={item.l} />
            </div>
          ))}
        </div>

        <div
          style={{ ...card, textAlign: "center" }}
          onMouseOver={e => e.currentTarget.style.transform = "translateY(-4px) scale(1.01)"}
          onMouseOut={e => e.currentTarget.style.transform = "translateY(0px) scale(1)"}
        >
          <h3>System Status</h3>
          <div style={{ marginTop: 10, fontWeight: "bold", color: statusInfo.color }}>
            {statusInfo.label}
          </div>
          <div style={{ marginTop: 10, color: colors.subtext }}>
            CPU: {cpu}% <br />
            RAM: {status?.ramUsedMB} MB
          </div>
        </div>

      </div>
    </div>
  );
}

export default Dashboard;