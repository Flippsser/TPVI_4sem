const celebritiesElement = document.getElementById("celebrities");
const lifeeventsElement = document.getElementById("lifeevents");

loadCelebrities();

async function loadCelebrities() {
    try {
        const response = await fetch("/api/Celebrities");
        if (!response.ok) {
            throw new Error(`GET /api/Celebrities failed: ${response.status}`);
        }

        const celebrities = await response.json();
        celebritiesElement.replaceChildren(...celebrities.map(createPhoto));
    }
    catch (error) {
        celebritiesElement.replaceChildren(createMessage("Не удалось загрузить знаменитостей. Проверьте подключение к MSSQL и выполните инициализацию БД."));
        console.error(error);
    }
}

function createPhoto(celebrity) {
    const photo = document.createElement("img");
    photo.className = "celebrity-photo";
    photo.src = `/api/Celebrities/photo/${encodeURIComponent(celebrity.reqPhotoPath ?? "")}`;
    photo.alt = celebrity.fullName;
    photo.title = celebrity.fullName;
    photo.tabIndex = 0;

    photo.addEventListener("click", () => showLifeevents(celebrity));
    photo.addEventListener("keydown", event => {
        if (event.key === "Enter" || event.key === " ") {
            event.preventDefault();
            showLifeevents(celebrity);
        }
    });

    return photo;
}

async function showLifeevents(celebrity) {
    lifeeventsElement.hidden = false;

    try {
        const response = await fetch(`/api/Lifeevents/Celebrities/${celebrity.id}`);
        if (!response.ok) {
            throw new Error(`GET /api/Lifeevents/Celebrities/${celebrity.id} failed: ${response.status}`);
        }

        const lifeevents = await response.json();
        const rows = lifeevents.map(lifeevent => createLifeeventRow(celebrity, lifeevent));
        lifeeventsElement.replaceChildren(...rows);
    }
    catch (error) {
        lifeeventsElement.replaceChildren(createMessage("Не удалось загрузить события."));
        console.error(error);
    }
}

function createLifeeventRow(celebrity, lifeevent) {
    const row = document.createElement("div");
    row.className = "lifeevent-row";

    const name = document.createElement("span");
    name.textContent = celebrity.fullName;

    const date = document.createElement("span");
    date.textContent = lifeevent.date ?? "";

    const description = document.createElement("span");
    description.textContent = lifeevent.description;

    row.append(name, date, description);
    return row;
}

function createMessage(text) {
    const message = document.createElement("p");
    message.className = "message";
    message.textContent = text;
    return message;
}
