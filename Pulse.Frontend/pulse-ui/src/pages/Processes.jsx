import { useState } from "react";
import { getColors } from "../theme";

function Processes({ processes, isDark }) {

  const colors = getColors(isDark);

  const [search, setSearch] = useState("");
  const [sortDesc, setSortDesc] = useState(true);

  const filtered = processes
    .filter(p =>
      p.name.toLowerCase().includes(search.toLowerCase())
    )
    .sort((a, b) =>
      sortDesc ? b.memoryMB - a.memoryMB : a.memoryMB - b.memoryMB
    );

  return (
    <div style={{ color: colors.text }}>

      {/* HEADER */}
      <div style={{ marginBottom: 20 }}>
        <h2>📊 Processes</h2>
        <p style={{ color: colors.subtext }}>
          Monitor and analyze running applications
        </p>
      </div>

      {/* CONTROLS */}
      <div style={{
        display: "flex",
        gap: 10,
        marginBottom: 15,
        flexWrap: "wrap"
      }}>
        <input
          placeholder="Search process..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          style={{
            flex: 1,
            padding: 10,
            borderRadius: 10,
            border: "none",
            background: colors.card,
            color: colors.text,
            outline: "none"
          }}
        />

        <button
          onClick={() => setSortDesc(!sortDesc)}
          style={{
            padding: "10px 14px",
            borderRadius: 10,
            border: "none",
            cursor: "pointer",
            background: colors.accent,
            color: "#fff"
          }}
        >
          Sort: {sortDesc ? "High → Low" : "Low → High"}
        </button>
      </div>

      {/* TABLE CARD */}
      <div style={{
        background: colors.card,
        borderRadius: 16,
        padding: 10,
        boxShadow: "0 10px 25px rgba(0,0,0,0.2)",
        overflowX: "auto"
      }}>

        <table style={{
          width: "100%",
          borderCollapse: "collapse",
          minWidth: 500
        }}>

          {/* HEADER */}
          <thead>
            <tr style={{
              textAlign: "left",
              borderBottom: `1px solid ${colors.hover}`,
              color: colors.subtext
            }}>
              <th style={th}>Name</th>
              <th style={th}>PID</th>
              <th style={th}>Memory</th>
              <th style={th}>CPU Time</th>
            </tr>
          </thead>

          {/* BODY */}
          <tbody>
            {filtered.map(p => {
              const isHeavy = p.memoryMB > 500;

              return (
                <tr
                  key={p.id}
                  style={{
                    borderBottom: `1px solid ${colors.bg}`,
                    background: isHeavy
                      ? "rgba(239,68,68,0.15)"
                      : "transparent",
                    transition: "0.2s"
                  }}
                  onMouseOver={(e) =>
                    (e.currentTarget.style.background = colors.hover)
                  }
                  onMouseOut={(e) =>
                    (e.currentTarget.style.background =
                      isHeavy
                        ? "rgba(239,68,68,0.15)"
                        : "transparent")
                  }
                >
                  <td style={td}>{p.name}</td>

                  <td style={{ ...td, color: colors.subtext }}>
                    {p.id}
                  </td>

                  <td style={{
                    ...td,
                    fontWeight: isHeavy ? "bold" : "normal",
                    color: isHeavy ? colors.danger : colors.text
                  }}>
                    {p.memoryMB} MB
                  </td>

                  <td style={{ ...td, color: colors.subtext }}>
                    {p.cpuTimeSeconds}s
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>

      </div>
    </div>
  );
}

/* STYLES */

const th = {
  padding: "12px 10px",
  fontWeight: "500"
};

const td = {
  padding: "12px 10px"
};

export default Processes;
