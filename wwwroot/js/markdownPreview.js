document.addEventListener('DOMContentLoaded', () => {
    const elements = document.getElementsByClassName('markdownContent');

    if (!elements.length) return;

    Array.from(elements).forEach(el => {
        const raw = el.textContent;
        const html = marked.parse(raw);
        el.innerHTML = html;
    });
});