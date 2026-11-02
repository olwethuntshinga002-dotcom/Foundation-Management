document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('loginForm');
    const username = document.getElementById('username');
    const password = document.getElementById('password');
    const toggle = document.getElementById('togglePassword');
    const remember = document.getElementById('rememberUsername');
    const errorBox = document.getElementById('loginError');
    const submitBtn = form.querySelector('.login-button');
    const KEY = 'fm_remembered_username';

    const showError = (msg) => {
        errorBox.textContent = msg;
        errorBox.classList.toggle('visible', !!msg);
    };

    // Show any server-side error already rendered by Razor
    showError(errorBox.textContent.trim());

    // Restore remembered username
    try {
        const saved = localStorage.getItem(KEY);
        if (saved) {
            username.value = saved;
            remember.checked = true;
            password.focus();
        } else {
            username.focus();
        }
    } catch { username.focus(); }

    // Show / hide password
    toggle.addEventListener('click', () => {
        const show = password.type === 'password';
        password.type = show ? 'text' : 'password';
        toggle.classList.toggle('is-visible', show);
        toggle.setAttribute('aria-pressed', String(show));
        toggle.setAttribute('aria-label', show ? 'Hide password' : 'Show password');
        password.focus();
    });

    // Clear error as the user types
    [username, password].forEach(el =>
        el.addEventListener('input', () => showError('')));

    form.addEventListener('submit', (e) => {
        if (!username.value.trim() || !password.value) {
            e.preventDefault();
            showError('Please enter both your username and password.');
            (username.value.trim() ? password : username).focus();
            return;
        }

        try {
            if (remember.checked) localStorage.setItem(KEY, username.value.trim());
            else localStorage.removeItem(KEY);
        } catch { /* storage unavailable */ }

        submitBtn.disabled = true;
        submitBtn.textContent = 'Signing in…';
    });
});