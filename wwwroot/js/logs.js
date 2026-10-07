const refreshButton = document.getElementById("refresh-logs");
const error = document.getElementById("error");
const formatter = new Intl.DateTimeFormat("ru-RU", {
    timeZone: "Europe/Moscow",
    dateStyle: "short",
    timeStyle: "medium"
});

async function loadLogs() {
    refreshButton.disabled = true;
    error.textContent = "";
    try {
        const response = await fetch("/api/logs");
        const entries = await response.json();
        if (!response.ok) throw new Error(entries.message || "Не удалось получить журнал");
        const tbody = document.getElementById("logs-body");
        tbody.replaceChildren();
        if (entries.length === 0) {
            const row = document.createElement("tr");
            const cell = document.createElement("td");
            cell.colSpan = 5;
            cell.textContent = "Запросов на обновление пока нет";
            row.appendChild(cell);
            tbody.appendChild(row);
        }
        for (const entry of entries) {
            const row = document.createElement("tr");
            const values = [formatter.format(new Date(entry.requestTime)), entry.method, entry.path,
                entry.statusCode, entry.success ? "Успешно" : "Неуспешно"];
            for (const value of values) {
                const cell = document.createElement("td");
                cell.textContent = value;
                row.appendChild(cell);
            }
            tbody.appendChild(row);
        }
    } catch (exception) {
        error.textContent = exception.message || "Не удалось получить журнал";
    } finally {
        refreshButton.disabled = false;
    }
}

refreshButton.addEventListener("click", loadLogs);
loadLogs();
