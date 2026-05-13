document.addEventListener('DOMContentLoaded', function() {
    const contentInput = document.getElementById('contentInput');
    const livePreview = document.getElementById('livePreview');

    function autoResize(textarea) {
        textarea.style.height = 'auto';
        textarea.style.height = textarea.scrollHeight + 'px';
    }

    function updatePreview() {
        const content = contentInput.value;
        livePreview.innerHTML = marked.parse(content);
        autoResize(contentInput);
    }

    contentInput.addEventListener('input', updatePreview);

    if (contentInput.value) {
        updatePreview();
    } else {
        autoResize(contentInput);
    }
});