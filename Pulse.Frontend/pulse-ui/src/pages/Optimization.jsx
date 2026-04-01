import { getColors } from "../theme";
function Optimization({ profile, switchProfile, isDark }) {

  const colors = getColors(isDark);

  const modeCard = (active) => ({
    background: active ? colors.accent : colors.card,
    color: active ? "#fff" : colors.text,
    padding: 20,
    borderRadius: 16,
    cursor: "pointer",
    transition: "0.2s",
    boxShadow: "0 10px 25px rgba(0,0,0,0.2)"
  });

  return (
    <div>
      <h2>⚙️ Optimization</h2>

      <div style={{
        display: "grid",
        gridTemplateColumns: "repeat(auto-fit, minmax(250px, 1fr))",
        gap: 20,
        marginTop: 20
      }}>
        {["balanced", "high"].map((mode) => (
          <div key={mode}
            style={modeCard(profile === mode)}
            onClick={() => switchProfile(mode)}
          >
            <h3>{mode.toUpperCase()}</h3>
          </div>
        ))}
      </div>

      <div style={{
        marginTop: 20,
        padding: 15,
        borderRadius: 12,
        background: colors.card
      }}>
        Current Profile: <b>{profile}</b>
      </div>
    </div>
  );
}

export default Optimization;