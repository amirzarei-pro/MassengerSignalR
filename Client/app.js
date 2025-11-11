// Simple client-only implementation targeting the RealtimeHub contract in RealtimeHub.cs
// This is a lightweight UI that connects to a SignalR hub at /realtime

const connection = new signalR.HubConnectionBuilder()
  .withUrl('/realtime')
  .configureLogging(signalR.LogLevel.Information)
  .build();

let me = null; // { userName, name }
let currentPeer = null; // peer userName

// UI refs
const usersEl = document.getElementById('users');
const convsEl = document.getElementById('conversations');
const messagesEl = document.getElementById('messages');
const chatWithEl = document.getElementById('chatWith');
const peerInfoEl = document.getElementById('peerInfo');

document.getElementById('btnRegister').addEventListener('click', async () => {
  const userName = document.getElementById('username').value.trim();
  const name = document.getElementById('name').value.trim() || userName;
  if (!userName) return alert('username required');
  const res = await connection.invoke('Register', userName, name);
  if (res && res.success) {
    me = res.user;
    chatWithEl.textContent = 'No chat selected';
    renderUser(me);
  }
});

document.getElementById('btnCheck').addEventListener('click', async () => {
  const userName = document.getElementById('username').value.trim();
  if (!userName) return alert('username required');
  const res = await connection.invoke('CheckUser', userName);
  alert(JSON.stringify(res));
});

document.getElementById('btnSend').addEventListener('click', async () => {
  const text = document.getElementById('messageText').value.trim();
  if (!text || !me || !currentPeer) return;
  const res = await connection.invoke('SendMessage', me.UserName || me.UserName, currentPeer, text);
  document.getElementById('messageText').value = '';
});

// hub callbacks
connection.on('Connected', (connectionId) => {
  console.info('connected as', connectionId);
});

connection.on('users', (list) => {
  renderUsers(list);
});

connection.on('conversations', (list) => {
  renderConversations(list);
});

connection.on('message', (msg) => {
  // show incoming message if it belongs to current chat
  const other = msg.From === (me?.UserName || me?.UserName) ? msg.To : msg.From;
  if (!currentPeer) return;
  const key = [me?.UserName?.toLowerCase(), currentPeer?.toLowerCase()].sort().join('__');
  const mkey = [msg.From.toLowerCase(), msg.To.toLowerCase()].sort().join('__');
  if (key === mkey) {
    appendMessage(msg);
  }
});

async function selectConversation(peer) {
  if (!me) return alert('register first');
  currentPeer = peer;
  chatWithEl.textContent = 'Chat with ' + peer;
  // load messages
  const res = await connection.invoke('GetMessages', me.UserName || me.UserName, peer);
  document.getElementById('peerInfo').textContent = res.user ? res.user.Name : '';
  renderMessages(res.messages || []);
}

function renderUsers(list) {
  usersEl.innerHTML = '';
  list.forEach(u => {
    const li = document.createElement('li');
    li.textContent = u.Name + ' (' + u.UserName + ')';
    li.addEventListener('click', () => selectConversation(u.UserName));
    usersEl.appendChild(li);
  });
}

function renderConversations(list) {
  convsEl.innerHTML = '';
  list.forEach(c => {
    const li = document.createElement('li');
    li.textContent = `${c.Peer} — ${c.LastText} (${c.TotalMessages})`;
    li.addEventListener('click', () => selectConversation(c.Peer));
    convsEl.appendChild(li);
  });
}

function renderMessages(list) {
  messagesEl.innerHTML = '';
  list.forEach(m => appendMessage(m));
  messagesEl.scrollTop = messagesEl.scrollHeight;
}

function appendMessage(msg) {
  const div = document.createElement('div');
  const fromMe = me && (msg.From.toLowerCase() === (me.UserName || me.UserName).toLowerCase());
  div.className = 'message ' + (fromMe ? 'me' : 'other');
  div.innerHTML = `<div class="text">${escapeHtml(msg.Text)}</div><div class="timestamp">${new Date(msg.SentAt).toLocaleString()}</div>`;
  messagesEl.appendChild(div);
  messagesEl.scrollTop = messagesEl.scrollHeight;
}

function renderUser(u) {
  // update personal UI state
  document.getElementById('username').value = u.UserName;
  document.getElementById('name').value = u.Name;
}

function escapeHtml(s) {
  return s.replace(/[&<>"']/g, (c) => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'})[c]);
}

// start connection
(async function () {
  try {
    await connection.start();
    console.log('SignalR connected');
    // request initial list of users
    // the hub sends users on connect/disconnect; we can also ask for conversations later
  } catch (err) {
    console.error(err);
    setTimeout(arguments.callee, 2000);
  }
})();
