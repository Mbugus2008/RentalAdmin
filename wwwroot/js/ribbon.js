(function () {
    const onReady = (callback) => {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', callback);
        } else {
            callback();
        }
    };

    onReady(() => {
        const tabButtons = Array.from(document.querySelectorAll('[data-office-tab]'));
        const panels = Array.from(document.querySelectorAll('[data-office-panel]'));

        if (tabButtons.length === 0 || panels.length === 0) {
            return;
        }

        const activate = (targetId) => {
            if (!targetId) {
                return;
            }

            tabButtons.forEach((button) => {
                const isActive = button.dataset.officeTab === targetId;
                button.classList.toggle('is-active', isActive);
                button.setAttribute('aria-selected', isActive ? 'true' : 'false');
                button.tabIndex = isActive ? 0 : -1;
            });

            panels.forEach((panel) => {
                const isActive = panel.dataset.officePanel === targetId;
                panel.classList.toggle('is-active', isActive);
                panel.tabIndex = isActive ? 0 : -1;
                if (isActive) {
                    panel.removeAttribute('hidden');
                } else {
                    panel.setAttribute('hidden', 'hidden');
                }
            });
        };

        const focusTab = (index) => {
            if (tabButtons.length === 0) {
                return;
            }

            const normalizedIndex = (index + tabButtons.length) % tabButtons.length;
            const button = tabButtons[normalizedIndex];
            button.focus();
            const target = button.dataset.officeTab;
            if (target) {
                activate(target);
            }
        };

        tabButtons.forEach((button, index) => {
            button.addEventListener('click', () => {
                const target = button.dataset.officeTab;
                if (target) {
                    activate(target);
                }
            });

            button.addEventListener('keydown', (event) => {
                if (event.key === 'ArrowRight') {
                    event.preventDefault();
                    focusTab(index + 1);
                } else if (event.key === 'ArrowLeft') {
                    event.preventDefault();
                    focusTab(index - 1);
                }
            });
        });

        const defaultButton = tabButtons.find((button) => button.classList.contains('is-active')) || tabButtons[0];
        if (defaultButton && defaultButton.dataset.officeTab) {
            activate(defaultButton.dataset.officeTab);
        }
    });
})();
