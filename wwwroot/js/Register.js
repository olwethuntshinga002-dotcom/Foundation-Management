document.addEventListener('DOMContentLoaded', () => {
    // ---- Donor / Foundation switch ----
    const tabs = document.querySelectorAll('.switch button');
    const panels = { donor: document.getElementById('panelDonor'), foundation: document.getElementById('panelFoundation') };

    tabs.forEach(btn => btn.addEventListener('click', () => {
        tabs.forEach(b => b.classList.toggle('active', b === btn));
        Object.entries(panels).forEach(([k, p]) => p.classList.toggle('active', k === btn.dataset.tab));
    }));

    // ---- Show / hide password ----
    document.querySelectorAll('.pw-toggle').forEach(btn => {
        btn.addEventListener('click', () => {
            const input = document.getElementById(btn.dataset.toggle);
            if (!input) return;
            const show = input.type === 'password';
            input.type = show ? 'text' : 'password';
            btn.textContent = show ? 'Hide' : 'Show';
        });
    });

    // ---- Password strength ----
    const levels = [
        { w: '20%', c: '#a63a2b', t: 'Too weak' },
        { w: '40%', c: '#c0702b', t: 'Weak' },
        { w: '65%', c: '#c58b2a', t: 'Fair' },
        { w: '85%', c: '#6f8f3a', t: 'Good' },
        { w: '100%', c: '#2f6b3a', t: 'Strong' }
    ];
    const score = (pw) => {
        let s = 0;
        if (pw.length >= 8) s++;
        if (pw.length >= 12) s++;
        if (/[a-z]/.test(pw) && /[A-Z]/.test(pw)) s++;
        if (/\d/.test(pw)) s++;
        if (/[^A-Za-z0-9]/.test(pw)) s++;
        return Math.min(s, 5);
    };

    document.querySelectorAll('input[data-strength]').forEach(input => {
        const bar = document.getElementById(input.dataset.strength);
        const label = document.getElementById(input.dataset.strength + 'Label');
        input.addEventListener('input', () => {
            if (!input.value) { bar.style.width = '0'; label.textContent = ''; return; }
            const lvl = levels[Math.max(score(input.value) - 1, 0)];
            bar.style.width = lvl.w;
            bar.style.background = lvl.c;
            label.textContent = lvl.t;
        });
    });

    // ---- Username preview from the foundation domain ----
    const domain = document.getElementById('Foundation_EmailDomain');
    const preview = document.getElementById('domainPreview');
    if (domain && preview) {
        const update = () => {
            const d = domain.value.trim().toLowerCase();
            preview.textContent = 'name@' + (d || 'mealcare.org');
        };
        domain.addEventListener('input', update);
        update();
    }

    // ---- Disable submit after click to prevent double posts ----
    document.querySelectorAll('form').forEach(f => f.addEventListener('submit', () => {
        const b = f.querySelector('button[type=submit]');
        if (b) { b.disabled = true; b.textContent = 'Please wait…'; }
    }));
});