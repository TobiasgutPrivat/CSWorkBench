// wwwroot/js/localStorageHelper.js
window.localStorageHelper = {
    getUserId: function () {
        return localStorage.getItem('userId');
    },
    setUserId: function (id) {
        localStorage.setItem('userId', id);
    }
};
