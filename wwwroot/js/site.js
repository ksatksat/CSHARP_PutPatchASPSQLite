document.addEventListener("DOMContentLoaded", () => {
    const listEl = document.getElementById("todo-list");
    const logEl = document.getElementById("log");

    const form = document.getElementById("edit-form");
    const editId = document.getElementById("edit-id");
    const editTitle = document.getElementById("edit-title");
    const editDescription = document.getElementById("edit-description");
    const editDone = document.getElementById("edit-done");
    const cancelBtn = document.getElementById("cancel-btn");

    let todos = [];
    let editingId = null;

    // Shows the last request/response so the HTTP method is easy to see
    function showLog(method, url, body, status, data, isError) {
        const lines = [`${method} ${url}`];
        if (body !== undefined) lines.push(`Request body: ${JSON.stringify(body)}`);
        lines.push("", `Response: HTTP ${status}`, JSON.stringify(data, null, 2));
        logEl.textContent = lines.join("\n");
        logEl.classList.toggle("error", isError);
    }

    // One helper for every request (GET, PUT, PATCH)
    async function request(method, url, body) {
        const options = { method, headers: {} };
        if (body !== undefined) {
            options.headers["Content-Type"] = "application/json";
            options.body = JSON.stringify(body);
        }

        const response = await fetch(url, options);
        let data = null;
        try { data = await response.json(); } catch { /* empty body */ }

        if (method !== "GET") showLog(method, url, body, response.status, data, !response.ok);
        if (!response.ok) throw new Error(data?.error || `HTTP ${response.status}`);
        return data;
    }

    function render() {
        listEl.innerHTML = "";

        todos.forEach(todo => {
            const li = document.createElement("li");
            if (todo.isDone) li.classList.add("done");

            const checkbox = document.createElement("input");
            checkbox.type = "checkbox";
            checkbox.checked = todo.isDone;
            checkbox.title = "Send PATCH with only isDone";
            checkbox.addEventListener("change", () => patchDone(todo.id, checkbox.checked));

            const text = document.createElement("div");
            text.className = "text";
            const title = document.createElement("span");
            title.className = "title";
            title.textContent = todo.title;
            const desc = document.createElement("span");
            desc.className = "desc";
            desc.textContent = todo.description;
            text.append(title, desc);

            const editBtn = document.createElement("button");
            editBtn.type = "button";
            editBtn.className = "secondary";
            editBtn.textContent = "Edit";
            editBtn.addEventListener("click", () => startEdit(todo));

            li.append(checkbox, text, editBtn);
            listEl.appendChild(li);
        });
    }

    function replaceTodo(updated) {
        todos = todos.map(t => (t.id === updated.id ? updated : t));
        render();
    }

    async function loadTodos() {
        try {
            todos = await request("GET", "/todos");
            render();
        } catch (err) {
            logEl.textContent = `Could not load items: ${err.message}`;
            logEl.classList.add("error");
        }
    }

    // HTTP PATCH: send ONLY the field that changed
    async function patchDone(id, isDone) {
        try {
            const updated = await request("PATCH", `/todos/${id}`, { isDone });
            replaceTodo(updated);
        } catch {
            render(); // put the checkbox back to the real state
        }
    }

    function startEdit(todo) {
        editingId = todo.id;
        editId.textContent = `#${todo.id}`;
        editTitle.value = todo.title;
        editDescription.value = todo.description;
        editDone.checked = todo.isDone;
        form.hidden = false;
        editTitle.focus();
    }

    function stopEdit() {
        editingId = null;
        form.hidden = true;
    }

    // HTTP PUT: send the COMPLETE item
    form.addEventListener("submit", async (event) => {
        event.preventDefault();
        if (editingId === null) return;

        const body = {
            title: editTitle.value,
            description: editDescription.value,
            isDone: editDone.checked
        };

        try {
            const updated = await request("PUT", `/todos/${editingId}`, body);
            replaceTodo(updated);
            stopEdit();
        } catch { /* the error is already shown in the log */ }
    });

    cancelBtn.addEventListener("click", stopEdit);

    loadTodos();
});
