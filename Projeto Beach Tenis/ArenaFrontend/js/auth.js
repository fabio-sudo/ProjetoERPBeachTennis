// auth.js
(function () {
    const token = localStorage.getItem('arena_token');
    const isLoginPage = window.location.pathname.endsWith('login.html') || window.location.pathname === '/' || window.location.pathname === '';

    if (!token && !isLoginPage) {
        window.location.href = 'login.html';
        return;
    }

    const originalFetch = window.fetch;
    window.fetch = async function () {
        let [resource, config] = arguments;

        if (!config) config = {};
        if (!config.headers) config.headers = {};

        const currentToken = localStorage.getItem('arena_token');
        if (currentToken) {
            // Check if headers is Headers object or plain object
            if (config.headers instanceof Headers) {
                config.headers.append('Authorization', `Bearer ${currentToken}`);
            } else {
                config.headers['Authorization'] = `Bearer ${currentToken}`;
            }
        }

        try {
            const response = await originalFetch(resource, config);

            if (response.status === 401 && !isLoginPage) {
                localStorage.removeItem('arena_token');
                localStorage.removeItem('arena_user');
                window.location.href = 'login.html';
            }

            return response;
        } catch (error) {
            throw error;
        }
    };

    // Auth utility functions
    window.auth = {
        logout: function () {
            localStorage.removeItem('arena_token');
            localStorage.removeItem('arena_user');
            localStorage.removeItem('arena_permissions');
            window.location.href = 'login.html';
        },
        getUser: function () {
            const u = localStorage.getItem('arena_user');
            return u ? JSON.parse(u) : null;
        },
        getPermissions: function () {
            const p = localStorage.getItem('arena_permissions');
            return p ? JSON.parse(p) : [];
        },
        hasPermission: function (screen) {
            const u = this.getUser();
            if (u && (u.role === 'Administrador' || u.role === 'Admin')) return true;
            const perms = this.getPermissions();
            return perms.includes(screen) || perms.includes('Admin');
        }
    };

    // UI Permissions Enforcer
    document.addEventListener('DOMContentLoaded', () => {
        if (!localStorage.getItem('arena_token') && isLoginPage) return;

        const u = window.auth.getUser();
        if (u && (u.role === 'Administrador' || u.role === 'Admin' || u.name === 'Admin')) return; // Allow Full Access

        const permissionMap = {
            'index.html': 'Dashboard',
            'students.html': 'Alunos',
            'customers.html': 'Clientes',
            'courts.html': 'Quadras',
            'schedule.html': 'Agendamentos',
            'bar.html': 'PDV',
            'tabs.html': 'Comandas',
            'cash_register.html': 'Caixa Diario',
            'sales_history.html': 'Historico',
            'inventory.html': 'Estoque',
            'purchase_orders.html': 'Pedidos de Compra',
            'reports.html': 'Relatorios',
            'admin.html': 'Admin',
            'admin.html#employees': 'Funcionarios'
        };

        const currentFile = window.location.pathname.split('/').pop() || 'index.html';
        const currentHash = window.location.hash;

        let reqPerm = permissionMap[currentFile + currentHash] || permissionMap[currentFile];
        if (reqPerm && !window.auth.hasPermission(reqPerm)) {
            // Unrestricted fallback route or boot them to login
            const fallback = window.auth.hasPermission('Dashboard') ? 'index.html' : 'login.html';
            if (currentFile !== 'login.html') window.location.href = fallback;
        }

        // Hide unauthorized sidebar links
        document.querySelectorAll('.sidebar-nav .nav-item').forEach(link => {
            const href = link.getAttribute('href');
            if (href) {
                const req = permissionMap[href];
                if (req && !window.auth.hasPermission(req)) {
                    link.style.display = 'none';
                }
            }
        });

        // Hide entire nav-sections if they are completely empty now
        document.querySelectorAll('.sidebar-nav .nav-section').forEach(sec => {
            const visibleLinks = Array.from(sec.querySelectorAll('.nav-item')).some(l => l.style.display !== 'none');
            if (!visibleLinks) {
                sec.style.display = 'none';
            }
        });
    });
})();
