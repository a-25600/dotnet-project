const API_BASE = 'http://localhost:5064/api';
let isLoginMode = true;

async function apiFetch(endpoint, method = 'GET', body = null) {
    const token = localStorage.getItem('jwt');
    const headers = { 'Content-Type': 'application/json' };
    if (token) headers['Authorization'] = `Bearer ${token}`;

    const options = { method, headers };
    if (body) options.body = JSON.stringify(body);

    const res = await fetch(`${API_BASE}${endpoint}`, options);

    if (res.status === 401) { logout(); throw new Error("Unauthorized"); }
    if (res.status === 403) { alert("Недостатньо прав для виконання цієї дії!"); throw new Error("Forbidden"); }
    if (res.status === 204) return null;

    let data;
    try { data = await res.json(); } catch (e) { data = null; }

    if (!res.ok) {
        if (data && data.errors) throw new Error("Помилка валідації:\n" + Object.values(data.errors).flat().join('\n'));
        throw new Error((data && data.message) || `HTTP Помилка: ${res.status}`);
    }
    return data;
}

function toggleAuthMode() {
    isLoginMode = !isLoginMode;
    document.getElementById('authTitle').innerText = isLoginMode ? "Вхід до системи" : "Реєстрація клієнта";
    document.getElementById('btnLogin').classList.toggle('hidden', !isLoginMode);
    document.getElementById('btnRegister').classList.toggle('hidden', isLoginMode);
    document.getElementById('registerFields').classList.toggle('hidden', isLoginMode);
    document.getElementById('authToggleLink').innerText = isLoginMode ? "Немає акаунту? Зареєструватися" : "Вже є акаунт? Увійти";
    document.getElementById('authError').classList.add('hidden');

    if (!isLoginMode) {
        document.getElementById('email').value = '';
        document.getElementById('password').value = '';
    } else {
        document.getElementById('email').value = 'admin@admin.com';
        document.getElementById('password').value = 'Admin123!';
    }
}

async function login() {
    try {
        const res = await fetch(`${API_BASE}/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                email: document.getElementById('email').value,
                password: document.getElementById('password').value
            })
        });
        const data = await res.json();
        if (!res.ok) throw new Error("Невірний логін/пароль");

        localStorage.setItem('jwt', data.token);
        localStorage.setItem('roles', JSON.stringify(data.roles));
        localStorage.setItem('email', document.getElementById('email').value);
        initApp();
    } catch (err) {
        document.getElementById('authError').innerText = err.message;
        document.getElementById('authError').classList.remove('hidden');
    }
}

async function register() {
    try {
        const payload = {
            fullName: document.getElementById('regName').value,
            email: document.getElementById('email').value,
            password: document.getElementById('password').value
        };
        const res = await fetch(`${API_BASE}/auth/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (!res.ok) {
            let errorMsg = "Помилка реєстрації.";
            try {
                const data = await res.json();
                errorMsg = data.message || (data.errors ? Object.values(data.errors).flat().join('\n') : errorMsg);
            } catch (e) { }
            throw new Error(errorMsg);
        }

        alert("Реєстрація успішна! Тепер ви можете увійти.");
        toggleAuthMode();
    } catch (err) {
        document.getElementById('authError').innerText = err.message;
        document.getElementById('authError').classList.remove('hidden');
    }
}

function logout() {
    localStorage.clear();
    document.getElementById('appSection').classList.add('hidden');
    document.getElementById('authSection').classList.remove('hidden');
}

window.onload = () => { if (localStorage.getItem('jwt')) initApp(); };

function initApp() {
    document.getElementById('authSection').classList.add('hidden');
    document.getElementById('appSection').classList.remove('hidden');
    document.getElementById('userEmailLabel').innerText = localStorage.getItem('email');

    const roles = JSON.parse(localStorage.getItem('roles') || '[]');
    const isAdmin = roles.includes('Administrator');
    const isAgent = roles.includes('Agent');

    document.getElementById('btnCreateProperty').classList.add('hidden');
    document.getElementById('btnCreateClient').classList.add('hidden');
    document.getElementById('btnCreateAgent').classList.add('hidden');
    document.getElementById('btnCreateDeal').classList.add('hidden');
    document.querySelector(`.nav-links button[data-target="clients"]`).classList.add('hidden');
    document.querySelector(`.nav-links button[data-target="agents"]`).classList.add('hidden');

    if (isAdmin) {
        document.getElementById('btnCreateProperty').classList.remove('hidden');
        document.getElementById('btnCreateClient').classList.remove('hidden');
        document.getElementById('btnCreateAgent').classList.remove('hidden');
        document.getElementById('btnCreateDeal').classList.remove('hidden');
        document.querySelector(`.nav-links button[data-target="clients"]`).classList.remove('hidden');
        document.querySelector(`.nav-links button[data-target="agents"]`).classList.remove('hidden');
    } else if (isAgent) {
        document.getElementById('btnCreateProperty').classList.remove('hidden');
        document.getElementById('btnCreateClient').classList.remove('hidden');
        document.getElementById('btnCreateDeal').classList.remove('hidden');
        document.querySelector(`.nav-links button[data-target="clients"]`).classList.remove('hidden');
    }

    loadCurrency();
    switchTab('properties');
}

function switchTab(tabName) {
    document.querySelectorAll('.nav-links button').forEach(b => b.classList.remove('active'));
    document.querySelector(`.nav-links button[data-target="${tabName}"]`).classList.add('active');
    document.querySelectorAll('.tab-content').forEach(tc => tc.classList.add('hidden'));
    document.getElementById(`tab-${tabName}`).classList.remove('hidden');

    if (tabName === 'properties') loadProperties();
    if (tabName === 'clients') loadClients();
    if (tabName === 'agents') loadAgents();
    if (tabName === 'deals') loadDeals();
}

function closeModal(modalId) {
    document.getElementById(modalId).classList.add('hidden');
}

async function loadCurrency() {
    try {
        const rates = await apiFetch('/currency/rates');
        const usd = rates.find(r => r.currencyCode === 'USD');
        const eur = rates.find(r => r.currencyCode === 'EUR');
        document.getElementById('ratesLoading').innerHTML =
            `<strong>USD:</strong> ${usd?.rate.toFixed(2)} ₴ | <strong>EUR:</strong> ${eur?.rate.toFixed(2)} ₴`;
    } catch (err) {
        document.getElementById('ratesLoading').innerText = "Помилка завантаження курсів";
    }
}

async function loadProperties() {
    try {
        const props = await apiFetch('/properties');
        const tbody = document.getElementById('propertiesTableBody');
        tbody.innerHTML = '';

        const roles = JSON.parse(localStorage.getItem('roles') || '[]');
        const isAdmin = roles.includes('Administrator');
        const isClient = roles.includes('Client');

        props.forEach(p => {
            const tr = document.createElement('tr');
            const statusText = p.isAvailable ? 'Доступно' : 'Недоступно';

            let actionsHtml = '';
            if (isAdmin || roles.includes('Agent')) {
                actionsHtml = `<button class="edit-btn" onclick='editProperty(${JSON.stringify(p)})'>Ред</button>
                               <button class="delete-btn" onclick="deleteProperty(${p.id})">Вид</button>`;
            } else if (isClient && p.isAvailable) {
                actionsHtml = `<button class="action-btn" style="padding: 5px 10px;" onclick="applyForProperty(${p.id}, ${p.price})">Залишити заявку</button>`;
            } else {
                actionsHtml = '<span style="color: gray; font-size:12px;">Тільки перегляд</span>';
            }

            tr.innerHTML = `
                <td>${p.id}</td><td>${p.address}</td><td>$${p.price}</td><td>${p.roomsCount}</td>
                <td>${statusText}</td><td>${actionsHtml}</td>
            `;
            tbody.appendChild(tr);
        });
    } catch (err) { console.error(err); }
}

function openPropertyModal() {
    document.getElementById('propId').value = '';
    document.getElementById('propAddress').value = '';
    document.getElementById('propPrice').value = '';
    document.getElementById('propRooms').value = '';
    document.getElementById('propertyModalTitle').innerText = "Новий об'єкт";
    document.getElementById('propertyModal').classList.remove('hidden');
}

function editProperty(prop) {
    document.getElementById('propId').value = prop.id;
    document.getElementById('propAddress').value = prop.address;
    document.getElementById('propPrice').value = prop.price;
    document.getElementById('propRooms').value = prop.roomsCount || '';
    document.getElementById('propType').value = prop.type || 'Apartment';
    document.getElementById('propStatus').value = prop.isAvailable ? 'Available' : 'Sold';
    document.getElementById('propertyModalTitle').innerText = "Редагування об'єкта";
    document.getElementById('propertyModal').classList.remove('hidden');
}

async function saveProperty() {
    const id = document.getElementById('propId').value;
    const payload = {
        address: document.getElementById('propAddress').value,
        price: Number(document.getElementById('propPrice').value) || 0,
        type: document.getElementById('propType').value,
        roomsCount: Number(document.getElementById('propRooms').value) || 0,
        isAvailable: document.getElementById('propStatus').value === 'Available'
    };
    try {
        if (id) await apiFetch(`/properties/${id}`, 'PUT', payload);
        else await apiFetch('/properties', 'POST', payload);
        closeModal('propertyModal');
        loadProperties();
    } catch (err) { alert(err.message); }
}

async function deleteProperty(id) {
    if (!confirm('Видалити цей об\'єкт нерухомості?')) return;
    try { await apiFetch(`/properties/${id}`, 'DELETE'); loadProperties(); } catch (err) { console.error(err); }
}

async function applyForProperty(propertyId, currentPrice) {
    if (!confirm('Ви дійсно хочете залишити заявку на цей об\'єкт? З вами зв\'яжеться перший вільний рієлтор.')) return;

    const payload = {
        propertyId: propertyId,
        clientId: 1,
        realtorId: null,
        price: currentPrice,
        status: "Pending"
    };

    try {
        await apiFetch('/deals', 'POST', payload);
        alert("Вашу заявку успішно створено! Очікуйте дзвінка рієлтора.");
    } catch (err) {
        alert("Помилка відправки заявки: " + err.message);
    }
}
async function loadClients() {
    try {
        const clients = await apiFetch('/clients');
        const tbody = document.getElementById('clientsTableBody');
        tbody.innerHTML = '';
        const isAdmin = JSON.parse(localStorage.getItem('roles') || '[]').includes('Administrator');

        clients.forEach(c => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td>${c.id}</td><td>${c.fullName || c.name}</td><td>${c.email}</td><td>${c.phoneNumber || c.phone}</td>
                <td>
                    ${isAdmin ? `<button class="edit-btn" onclick='editClient(${JSON.stringify(c)})'>Ред</button>
                                 <button class="delete-btn" onclick="deleteClient(${c.id})">Вид</button>` : '-'}
                </td>`;
            tbody.appendChild(tr);
        });
    } catch (err) { console.error(err); }
}

function openClientModal() {
    document.getElementById('clientId').value = '';
    document.getElementById('clientName').value = '';
    document.getElementById('clientEmail').value = '';
    document.getElementById('clientPhone').value = '';
    document.getElementById('clientModalTitle').innerText = "Новий клієнт";
    document.getElementById('clientModal').classList.remove('hidden');
}

function editClient(client) {
    document.getElementById('clientId').value = client.id;
    document.getElementById('clientName').value = client.fullName || client.name;
    document.getElementById('clientEmail').value = client.email;
    document.getElementById('clientPhone').value = client.phoneNumber || client.phone;
    document.getElementById('clientModalTitle').innerText = "Редагування клієнта";
    document.getElementById('clientModal').classList.remove('hidden');
}

async function saveClient() {
    const id = document.getElementById('clientId').value;
    const payload = {
        fullName: document.getElementById('clientName').value,
        email: document.getElementById('clientEmail').value,
        phoneNumber: document.getElementById('clientPhone').value
    };
    try {
        if (id) await apiFetch(`/clients/${id}`, 'PUT', payload);
        else await apiFetch('/clients', 'POST', payload);
        closeModal('clientModal');
        loadClients();
    } catch (err) { alert(err.message); }
}

async function deleteClient(id) {
    if (!confirm('Видалити клієнта?')) return;
    try { await apiFetch(`/clients/${id}`, 'DELETE'); loadClients(); } catch (err) { alert(err.message); }
}

async function loadAgents() {
    try {
        const agents = await apiFetch('/agents');
        const tbody = document.getElementById('agentsTableBody');
        tbody.innerHTML = '';
        const isAdmin = JSON.parse(localStorage.getItem('roles') || '[]').includes('Administrator');

        agents.forEach(a => {
            const tr = document.createElement('tr');
            const status = a.isActive ? 'Активний' : 'Неактивний';
            tr.innerHTML = `
                <td>${a.id}</td><td>${a.fullName || a.name}</td><td>${a.email}</td><td>${a.phoneNumber || a.phone}</td>
                <td>${a.licenseNumber}</td><td>${status}</td>
                <td>
                    ${isAdmin ? `<button class="edit-btn" onclick='editAgent(${JSON.stringify(a)})'>Ред</button>
                                 <button class="delete-btn" onclick="deleteAgent(${a.id})">Вид</button>` : '-'}
                </td>`;
            tbody.appendChild(tr);
        });
    } catch (err) { console.error(err); }
}

function openAgentModal() {
    document.getElementById('agentId').value = '';
    document.getElementById('agentName').value = '';
    document.getElementById('agentEmail').value = '';
    document.getElementById('agentPhone').value = '';
    document.getElementById('agentLicense').value = '';
    document.getElementById('agentActive').value = 'true';
    document.getElementById('agentModalTitle').innerText = "Новий рієлтор";
    document.getElementById('agentModal').classList.remove('hidden');
}

function editAgent(agent) {
    document.getElementById('agentId').value = agent.id;
    document.getElementById('agentName').value = agent.fullName || agent.name;
    document.getElementById('agentEmail').value = agent.email;
    document.getElementById('agentPhone').value = agent.phoneNumber || agent.phone;
    document.getElementById('agentLicense').value = agent.licenseNumber;
    document.getElementById('agentActive').value = agent.isActive ? 'true' : 'false';
    document.getElementById('agentModalTitle').innerText = "Редагування рієлтора";
    document.getElementById('agentModal').classList.remove('hidden');
}

async function saveAgent() {
    const id = document.getElementById('agentId').value;
    const payload = {
        fullName: document.getElementById('agentName').value,
        email: document.getElementById('agentEmail').value,
        phoneNumber: document.getElementById('agentPhone').value,
        licenseNumber: document.getElementById('agentLicense').value,
        isActive: document.getElementById('agentActive').value === 'true'
    };
    try {
        if (id) await apiFetch(`/agents/${id}`, 'PUT', payload);
        else await apiFetch('/agents', 'POST', payload);
        closeModal('agentModal');
        loadAgents();
    } catch (err) { alert(err.message); }
}

async function deleteAgent(id) {
    if (!confirm('Видалити рієлтора?')) return;
    try { await apiFetch(`/agents/${id}`, 'DELETE'); loadAgents(); } catch (err) { alert(err.message); }
}

async function loadDeals() {
    try {
        const deals = await apiFetch('/deals');
        const tbody = document.getElementById('dealsTableBody');
        tbody.innerHTML = '';

        const roles = JSON.parse(localStorage.getItem('roles') || '[]');
        const isAdmin = roles.includes('Administrator');
        const isAgent = roles.includes('Agent');
        const myEmail = localStorage.getItem('email');

        let myAgentId = null;
        if (isAgent && !isAdmin) {
            const agents = await apiFetch('/agents');
            const me = agents.find(a => a.email === myEmail);
            if (me) myAgentId = me.id;
        }

        deals.forEach(d => {
            const tr = document.createElement('tr');

            const canEdit = isAdmin || (isAgent && (!d.realtorId || d.realtorId === myAgentId));

            tr.innerHTML = `
                <td>${d.id}</td><td>${d.propertyId}</td><td>${d.clientId || 'Auto'}</td><td>${d.realtorId || d.agentId || 'Не призначено'}</td>
                <td>$${d.price}</td><td>${d.status}</td>
                <td>
                    ${canEdit
                    ? `<button class="edit-btn" onclick='editDeal(${JSON.stringify(d)})'>Ред</button>
                           ${isAdmin ? `<button class="delete-btn" onclick="deleteDeal(${d.id})">Вид</button>` : ''}`
                    : '<span style="color: gray; font-size:12px;">Тільки перегляд</span>'
                }
                </td>`;
            tbody.appendChild(tr);
        });
    } catch (err) { console.error(err); }
}

async function populateDealDropdowns() {
    try {
        const props = await apiFetch('/properties');
        const clients = await apiFetch('/clients');
        const agents = await apiFetch('/agents');

        const propSelect = document.getElementById('dealPropertyId');
        propSelect.innerHTML = props.filter(p => p.isAvailable).map(p => `<option value="${p.id}">${p.address} (ID: ${p.id})</option>`).join('');

        const clientSelect = document.getElementById('dealClientId');
        clientSelect.innerHTML = clients.map(c => `<option value="${c.id}">${c.fullName || c.name}</option>`).join('');

        const agentSelect = document.getElementById('dealAgentId');
        agentSelect.innerHTML = '<option value="">-- Вільна заявка --</option>' +
            agents.map(a => `<option value="${a.id}">${a.fullName || a.name}</option>`).join('');
    } catch (err) { console.error(err); }
}

async function editDeal(deal) {
    await populateDealDropdowns();
    document.getElementById('dealId').value = deal.id;
    document.getElementById('dealPropertyId').innerHTML += `<option value="${deal.propertyId}" selected>Поточний об'єкт (ID: ${deal.propertyId})</option>`;
    document.getElementById('dealClientId').value = deal.clientId;
    document.getElementById('dealAgentId').value = deal.realtorId || deal.agentId || '';
    document.getElementById('dealPrice').value = deal.price;
    document.getElementById('dealStatus').value = deal.status || 'Pending';
    document.getElementById('dealModalTitle').innerText = "Редагування угоди";

    const roles = JSON.parse(localStorage.getItem('roles') || '[]');
    const isAdmin = roles.includes('Administrator');

    const propSelect = document.getElementById('dealPropertyId');
    const clientSelect = document.getElementById('dealClientId');
    const agentSelect = document.getElementById('dealAgentId');

    if (!isAdmin) {
        propSelect.disabled = true;
        clientSelect.disabled = true;

        if (deal.realtorId || deal.agentId) {
            agentSelect.disabled = true;
        } else {
            agentSelect.disabled = false;
        }
    } else {
        propSelect.disabled = false;
        clientSelect.disabled = false;
        agentSelect.disabled = false;
    }

    document.getElementById('dealModal').classList.remove('hidden');
}

async function openDealModal() {
    await populateDealDropdowns();
    document.getElementById('dealId').value = '';
    document.getElementById('dealPrice').value = '';
    document.getElementById('dealStatus').value = 'Pending';
    document.getElementById('dealModalTitle').innerText = "Нова угода";
    document.getElementById('dealPropertyId').disabled = false;
    document.getElementById('dealClientId').disabled = false;
    document.getElementById('dealAgentId').disabled = false;

    document.getElementById('dealModal').classList.remove('hidden');
}
async function saveDeal() {
    const id = document.getElementById('dealId').value;
    const agentVal = document.getElementById('dealAgentId').value;

    const payload = {
        propertyId: Number(document.getElementById('dealPropertyId').value),
        clientId: Number(document.getElementById('dealClientId').value),
        realtorId: agentVal ? Number(agentVal) : null,
        price: Number(document.getElementById('dealPrice').value),
        status: document.getElementById('dealStatus').value
    };
    try {
        if (id) await apiFetch(`/deals/${id}`, 'PUT', payload);
        else await apiFetch('/deals', 'POST', payload);
        closeModal('dealModal');
        loadDeals();
    } catch (err) { alert(err.message); }
}

async function deleteDeal(id) {
    if (!confirm('Видалити угоду?')) return;
    try { await apiFetch(`/deals/${id}`, 'DELETE'); loadDeals(); } catch (err) { alert(err.message); }
}