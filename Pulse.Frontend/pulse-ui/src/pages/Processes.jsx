import { useState } from "react";

function Processes({ processes, isDark }) {

  const bg = isDark ? "#2c2c2c" : "#fff";
  const text = isDark ? "#fff" : "#000";
  const rowHover = isDark ? "#3a3a3a" : "#f1f1f1";

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
    <div style={{ color: text }}>
      <h2>Processes</h2>

      {/* CONTROLS */}
      <div style={{
        display: "flex",
        gap: 10,
        marginTop: 10,
        flexWrap: "wrap"
      }}>
        <input
          placeholder="Search..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          style={{
            padding: 8,
            borderRadius: 6,
            border: "1px solid #ccc",
            background: bg,
            color: text,
            flex: 1,
            minWidth: 150
          }}
        />

        <button
          onClick={() => setSortDesc(!sortDesc)}
          style={{
            padding: "8px 12px",
            borderRadius: 6,
            border: "none",
            cursor: "pointer"
          }}
        >
          Sort: {sortDesc ? "High → Low" : "Low → High"}
        </button>
      </div>

      {/* TABLE CONTAINER */}
      <div style={{
        marginTop: 15,
        maxHeight: "70vh",
        overflowY: "auto",
        background: bg,
        borderRadius: 10,
        padding: 10,
        boxShadow: "0 2px 10px rgba(0,0,0,0.1)"
      }}>

        {/* HORIZONTAL SCROLL*/}
        <div style={{ overflowX: "auto" }}>

          <table style={{
            width: "100%",
            minWidth: 500,
            borderCollapse: "collapse"
          }}>
            <thead>
              <tr style={{ background: isDark ? "#444" : "#eee" }}>
                <th style={{ padding: 8 }}>Name</th>
                <th style={{ padding: 8 }}>PID</th>
                <th style={{ padding: 8 }}>Memory</th>
                <th style={{ padding: 8 }}>CPU</th>
              </tr>
            </thead>

            <tbody>
              {filtered.map(p => (
                <tr
                  key={p.id}
                  style={{
                    borderBottom: "1px solid #555",
                    backgroundColor: p.memoryMB > 500
                      ? (isDark ? "#5c2b2b" : "#ffe6e6")
                      : "transparent"
                  }}
                  onMouseOver={(e) =>
                    (e.currentTarget.style.backgroundColor = rowHover)
                  }
                  onMouseOut={(e) =>
                    (e.currentTarget.style.backgroundColor =
                      p.memoryMB > 500
                        ? (isDark ? "#5c2b2b" : "#ffe6e6")
                        : "transparent")
                  }
                >
                  <td style={{ padding: 8 }}>{p.name}</td>
                  <td style={{ padding: 8 }}>{p.id}</td>
                  <td style={{
                    padding: 8,
                    fontWeight: p.memoryMB > 500 ? "bold" : "normal",
                    color: p.memoryMB > 500 ? "red" : text
                  }}>
                    {p.memoryMB} MB
                  </td>
                  <td style={{ padding: 8 }}>{p.cpuTimeSeconds}</td>
                </tr>
              ))}
            </tbody>
          </table>

        </div>
      </div>
    </div>
  );
}

export default Processes;