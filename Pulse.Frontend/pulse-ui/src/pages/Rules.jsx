import { useState } from "react";
import { getColors } from "../theme";

function Rules({
    rules,
    newRule,
    setNewRule,
    addRule,
    deleteRule,
    editingRule,
    setEditingRule,
    updateRule,
    isDark
}) {

    const colors = getColors(isDark);
    const [inputValue, setInputValue] = useState("");

    return (
        <div style={{ color: colors.text }}>

            {/* HEADER */}
            <div style={{ marginBottom: 20 }}>
                <h2>🧠 Rules</h2>
                <p style={{ color: colors.subtext }}>
                    Automate system behavior based on running applications
                </p>
            </div>

            {/* ADD RULE */}
            <div style={section(colors)}>
                <h3>Add Rule</h3>

                <input
                    placeholder="Trigger Process (e.g. chrome)"
                    value={newRule.triggerProcess}
                    onChange={(e) =>
                        setNewRule({ ...newRule, triggerProcess: e.target.value })
                    }
                    style={input(colors)}
                />

                <select
                    value={newRule.targetProfile}
                    onChange={(e) =>
                        setNewRule({ ...newRule, targetProfile: e.target.value })
                    }
                    style={input(colors)}
                >
                    <option value="balanced">Balanced</option>
                    <option value="high">High Performance</option>
                </select>

                <input
                    placeholder="Add app to close (Enter)"
                    value={inputValue}
                    onChange={(e) => setInputValue(e.target.value)}
                    onKeyDown={(e) => {
                        if (e.key === "Enter") {
                            e.preventDefault();
                            const value = inputValue.trim().toLowerCase();
                            if (!value) return;

                            const existing = newRule.closeApps || [];

                            if (!existing.includes(value)) {
                                setNewRule({
                                    ...newRule,
                                    closeApps: [...existing, value]
                                });
                            }

                            setInputValue("");
                        }
                    }}
                    style={input(colors)}
                />

                <div style={{ marginTop: 10 }}>
                    {newRule.closeApps?.map((app, i) => (
                        <span
                            key={i}
                            onClick={() => {
                                const updated = newRule.closeApps.filter(a => a !== app);
                                setNewRule({ ...newRule, closeApps: updated });
                            }}
                            style={chip(colors)}
                        >
                            {app} ✕
                        </span>
                    ))}
                </div>

                <button onClick={addRule} style={primaryBtn(colors)}>
                    Add Rule
                </button>
            </div>

            {/* RULE LIST */}
            <div style={{ marginTop: 25 }}>

                {rules.length === 0 && (
                    <div style={{ textAlign: "center", color: colors.subtext }}>
                        <h3>No rules yet</h3>
                        <p>Create your first automation rule</p>
                    </div>
                )}

                <div style={{
                    display: "grid",
                    gridTemplateColumns: "repeat(auto-fit, minmax(250px, 1fr))",
                    gap: 15
                }}>
                    {rules.map((r) => (
                        <div key={r.id} style={card(colors)}
                            onMouseOver={e => e.currentTarget.style.transform = "translateY(-4px) scale(1.01)"}
                            onMouseOut={e => e.currentTarget.style.transform = "translateY(0px) scale(1)"}
                        >
                            <h3>{r.triggerProcess?.toUpperCase() || "UNKNOWN"}</h3>

                            <p style={{ color: colors.subtext }}>
                                {r.targetProfile === "high" ? "High Performance" : "Balanced"}
                            </p>

                            <div style={{ marginTop: 10 }}>
                                {r.closeApps?.length > 0
                                    ? r.closeApps.map((app, i) => (
                                        <span key={i} style={chip(colors)}>{app}</span>
                                    ))
                                    : <span style={{ color: colors.subtext }}>No apps</span>}
                            </div>

                            <div style={{ marginTop: 15 }}>
                                <button onClick={() => setEditingRule({ ...r })} style={editBtn}>
                                    Edit
                                </button>

                                <button onClick={() => deleteRule(r.id)} style={deleteBtn(colors)}>
                                    Delete
                                </button>
                            </div>
                        </div>
                    ))}
                </div>
            </div>

            {/* MODAL */}
            {editingRule && (
                <div style={modalOverlay}>
                    <div style={modalBox(colors)}>
                        <h3>Edit Rule</h3>

                        <input
                            value={editingRule.triggerProcess}
                            onChange={(e) =>
                                setEditingRule({ ...editingRule, triggerProcess: e.target.value })
                            }
                            style={input(colors)}
                        />

                        <select
                            value={editingRule.targetProfile}
                            onChange={(e) =>
                                setEditingRule({ ...editingRule, targetProfile: e.target.value })
                            }
                            style={input(colors)}
                        >
                            <option value="balanced">Balanced</option>
                            <option value="high">High Performance</option>
                        </select>

                        <div style={{ marginTop: 15 }}>
                            <button onClick={() => updateRule(editingRule.id, editingRule)} style={primaryBtn(colors)}>
                                Save
                            </button>

                            <button onClick={() => setEditingRule(null)} style={secondaryBtn(colors)}>
                                Cancel
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

/* STYLES */

const section = (c) => ({
    background: c.card,
    padding: 20,
    borderRadius: 16
});

const card = (c) => ({
    background: c.card,
    padding: 15,
    borderRadius: 16,
    transition: "0.2s"
});

const input = (c) => ({
    width: "100%",
    padding: 10,
    marginTop: 10,
    borderRadius: 10,
    border: "none",
    background: c.bg,
    color: c.text
});

const chip = (c) => ({
    marginRight: 6,
    padding: "4px 10px",
    borderRadius: 12,
    background: c.accent,
    color: "#fff",
    fontSize: 12,
    cursor: "pointer"
});

const primaryBtn = (c) => ({
    marginTop: 15,
    padding: "10px 14px",
    borderRadius: 10,
    border: "none",
    background: c.accent,
    color: "#fff",
    cursor: "pointer"
});

const secondaryBtn = (c) => ({
    marginLeft: 10,
    padding: "10px 14px",
    borderRadius: 10,
    border: "none",
    background: c.hover,
    color: c.text
});

const editBtn = {
    marginRight: 8,
    padding: "6px 10px",
    borderRadius: 8,
    border: "none",
    background: "#f59e0b",
    color: "#fff",
    cursor: "pointer"
};

const deleteBtn = (c) => ({
    padding: "6px 10px",
    borderRadius: 8,
    border: "none",
    background: c.danger,
    color: "#fff",
    cursor: "pointer"
});

const modalOverlay = {
    position: "fixed",
    inset: 0,
    background: "rgba(0,0,0,0.7)",
    display: "flex",
    justifyContent: "center",
    alignItems: "center"
};

const modalBox = (c) => ({
    background: c.card,
    padding: 25,
    borderRadius: 16,
    width: 400
});

export default Rules;