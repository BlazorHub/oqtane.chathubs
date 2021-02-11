window.addEventListener('DOMContentLoaded', () => {

    document.addEventListener('swiped-right', function (e) {

        document.getElementById('blazortouchmenuid').style.display = "block";
    });

    document.addEventListener('swiped-left', function (e) {

        document.getElementById('blazortouchmenuid').style.display = "none";
    });

});