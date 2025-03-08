//TODO: refactor without jQuery
document.addEventListener('click', function(event) {
    if (event.target.classList.contains('iconpicker-item')) {
        event.preventDefault();
    }
});
