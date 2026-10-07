const form = document.getElementById("request-form");
const mode = form.dataset.mode;
const idInput = document.getElementById("user-id");
const nameInput = document.getElementById("name");
const emailInput = document.getElementById("email");
const ageInput = document.getElementById("age");
const sendButton = document.getElementById("send-button");
const error = document.getElementById("error");
const usersError = document.getElementById("users-error");
const refreshButton = document.getElementById("refresh-users");
let users = [];

function fillFields() {
    const user = users.find(item => item.id === Number(idInput.value));
    if (user) {
        nameInput.value = user.name;
        emailInput.value = user.email;
        if (ageInput) ageInput.value = user.age;
    }
}

async function loadUsers(fillForm = false) {
    usersError.textContent = "";
    refreshButton.disabled = true;
    try {
        const response = await fetch("/api/users");
        const data = await response.json();
        if (!response.ok) throw new Error(data.message || "Не удалось получить пользователей");
        users = data;
        const tbody = document.getElementById("users-body");
        tbody.replaceChildren();
        for (const user of users) {
            const row = document.createElement("tr");
            for (const value of [user.id, user.name, user.email, user.age]) {
                const cell = document.createElement("td");
                cell.textContent = value;
                row.appendChild(cell);
            }
            tbody.appendChild(row);
        }
        if (fillForm) fillFields();
    } catch (exception) {
        usersError.textContent = exception.message || "Не удалось получить пользователей";
    } finally {
        refreshButton.disabled = false;
    }
}

idInput.addEventListener("change", fillFields);
refreshButton.addEventListener("click", () => loadUsers());

form.addEventListener("submit", async event => {
    event.preventDefault();
    error.textContent = "";
    const id = Number(idInput.value);
    if (!Number.isInteger(id) || id < 1 || id > 2147483647) {
        error.textContent = "Введите целый ID от 1 до 2147483647";
        return;
    }
    const payload = { name: nameInput.value, email: emailInput.value };
    if (ageInput) {
        const age = Number(ageInput.value);
        if (!Number.isInteger(age) || age < 0 || age > 120) {
            error.textContent = "Введите целый возраст от 0 до 120";
            return;
        }
        payload.age = age;
    }
    const suffix = mode === "email" ? "/email" : mode === "transaction" ? "/transaction" : "";
    const url = `/api/users/${id}${suffix}`;
    sendButton.disabled = true;
    refreshButton.disabled = true;
    document.getElementById("response-url").textContent = `PUT ${url}`;
    document.getElementById("response-status").textContent = "Ожидание ответа";
    document.getElementById("response-body").textContent = "";
    try {
        const response = await fetch(url, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });
        const body = await response.text();
        document.getElementById("response-status").textContent = `${response.status} ${response.statusText}`.trim();
        let formatted = body || "Ответ без тела";
        if (body) {
            try {
                formatted = JSON.stringify(JSON.parse(body), null, 2);
            } catch {
                formatted = body;
            }
        }
        document.getElementById("response-body").textContent = formatted;
        await loadUsers();
    } catch {
        error.textContent = "Не удалось отправить запрос, проверьте доступность сервера";
        document.getElementById("response-status").textContent = "Ответ не получен";
    } finally {
        sendButton.disabled = false;
        refreshButton.disabled = false;
    }
});

loadUsers(true).finally(() => { sendButton.disabled = false; });
