document.addEventListener('click', (e) => {
    if (!e.target.closest('.iconpicker-item')) {
        return;
    }
    e.preventDefault();
    e.stopPropagation();
});
