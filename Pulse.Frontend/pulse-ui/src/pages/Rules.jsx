import { useState } from "react";

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
    const bg = isDark ? "#2c2c2c" : "#fff";
    const text = isDark ? "#fff" : "#000";

    const [inputValue, setInputValue] = useState("");

    return (
        <div style={{ color: text }}>
            <h2>Rules</h2>

            {/* ADD RULE */}
            <div style={{
                background: bg,
                padding: 20,
                borderRadius: 12,
                marginTop: 20
            }}>
                <h3>Add Rule</h3>

                {/* Trigger */}
                <input
                    placeholder="Trigger Process (e.g. chrome)"
                    value={newRule.triggerProcess}
                    onChange={(e) =>
                        setNewRule({ ...newRule, triggerProcess: e.target.value })
                    }
                    style={{ padding: 8, marginBottom: 10, width: "100%" }}
                />

                {/* Profile */}
                <select
                    value={newRule.targetProfile}
                    onChange={(e) =>
                        setNewRule({ ...newRule, targetProfile: e.target.value })
                    }
                    style={{ padding: 8, marginBottom: 10, width: "100%" }}
                >
                    <option value="balanced">Balanced</option>
                    <option value="high">High Performance</option>
                </select>

                {/* Tag Input */}
                <input
                    placeholder="Add app to close (press Enter)"
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
                    style={{ padding: 8, width: "100%" }}
                />

                {/* Tags */}
                <div style={{ marginTop: 10 }}>
                    {newRule.closeApps?.map((app, i) => (
                        <span
                            key={i}
                            onClick={() => {
                                const updated = newRule.closeApps.filter(a => a !== app);

                                setNewRule({
                                    ...newRule,
                                    closeApps: updated
                                });
                            }}
                            style={{
                                marginRight: 8,
                                padding: "5px 10px",
                                borderRadius: 10,
                                background: "#4a90e2",
                                color: "#fff",
                                display: "inline-block",
                                cursor: "pointer"
                            }}
                        >
                            {app} ✕
                        </span>
                    ))}
                </div>

                {/* Add Button */}
                <button
                    onClick={() => {
                        if (!newRule.triggerProcess.trim()) {
                            alert("Enter a trigger process");
                            return;
                        }
                        addRule();
                    }}
                    style={{
                        marginTop: 15,
                        padding: "8px 12px",
                        borderRadius: 6,
                        border: "none",
                        cursor: "pointer",
                        background: "#4a90e2",
                        color: "#fff"
                    }}
                >
                    Add Rule
                </button>
            </div>

            {/* RULE LIST */}
            <div style={{
                background: bg,
                padding: 20,
                borderRadius: 12,
                marginTop: 20
            }}>
                <h3>Existing Rules</h3>

                {rules.length === 0 && <p>No rules yet</p>}

                {rules.map((r) => (
                    <div key={r.id} style={{
                        marginBottom: 10,
                        padding: 10,
                        borderRadius: 8,
                        background: isDark ? "#3a3a3a" : "#f5f5f5"
                    }}>
                        <b>{r.triggerProcess}</b> → {r.targetProfile}
                        <br />
                        Close: {r.closeApps?.join(", ") || "None"}

                        <br />

                        {/* DELETE */}
                        <button
                            onClick={() => deleteRule(r.id)}
                            style={{
                                marginTop: 5,
                                padding: "5px 10px",
                                borderRadius: 5,
                                border: "none",
                                cursor: "pointer",
                                background: "#ff4d4d",
                                color: "#fff"
                            }}
                        >
                            Delete
                        </button>

                        {/* EDIT */}
                        <button
                            onClick={() => setEditingRule({ ...r })}
                            style={{
                                marginTop: 5,
                                marginLeft: 10,
                                padding: "5px 10px",
                                borderRadius: 5,
                                border: "none",
                                cursor: "pointer",
                                background: "#ffaa00",
                                color: "#fff"
                            }}
                        >
                            Edit
                        </button>
                    </div>
                ))}
            </div>

            {/* EDIT MODAL */}
            {editingRule && (
                <div style={{
                    position: "fixed",
                    top: 0,
                    left: 0,
                    width: "100%",
                    height: "100%",
                    background: "rgba(0,0,0,0.5)",
                    display: "flex",
                    justifyContent: "center",
                    alignItems: "center"
                }}>
                    <div style={{
                        background: bg,
                        padding: 20,
                        borderRadius: 12,
                        width: 400
                    }}>
                        <h3>Edit Rule</h3>

                        <input
                            value={editingRule.triggerProcess}
                            onChange={(e) =>
                                setEditingRule({
                                    ...editingRule,
                                    triggerProcess: e.target.value
                                })
                            }
                            style={{ width: "100%", marginBottom: 10 }}
                        />

                        <select
                            value={editingRule.targetProfile}
                            onChange={(e) =>
                                setEditingRule({
                                    ...editingRule,
                                    targetProfile: e.target.value
                                })
                            }
                            style={{ width: "100%", marginBottom: 10 }}
                        >
                            <option value="balanced">Balanced</option>
                            <option value="high">High Performance</option>
                        </select>

                        <input
                            placeholder="Add app (Enter)"
                            onKeyDown={(e) => {
                                if (e.key === "Enter") {
                                    e.preventDefault();
                                    const value = e.target.value.trim().toLowerCase();
                                    if (!value) return;

                                    if (!editingRule.closeApps.includes(value)) {
                                        setEditingRule({
                                            ...editingRule,
                                            closeApps: [...editingRule.closeApps, value]
                                        });
                                    }

                                    e.target.value = "";
                                }
                            }}
                            style={{ width: "100%" }}
                        />

                        <div style={{ marginTop: 10 }}>
                            {editingRule.closeApps.map((app, i) => (
                                <span
                                    key={i}
                                    onClick={() =>
                                        setEditingRule({
                                            ...editingRule,
                                            closeApps: editingRule.closeApps.filter(a => a !== app)
                                        })
                                    }
                                    style={{
                                        marginRight: 8,
                                        padding: "5px 10px",
                                        borderRadius: 10,
                                        background: "#4a90e2",
                                        color: "#fff",
                                        cursor: "pointer"
                                    }}
                                >
                                    {app} ✕
                                </span>
                            ))}
                        </div>

                        <div style={{ marginTop: 15 }}>
                            <button
                                onClick={() => updateRule(editingRule.id, editingRule)}
                                style={{ marginRight: 10 }}
                            >
                                Save
                            </button>

                            <button onClick={() => setEditingRule(null)}>
                                Cancel
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default Rules;