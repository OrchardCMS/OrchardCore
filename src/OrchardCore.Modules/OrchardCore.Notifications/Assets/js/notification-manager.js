notificationManager = function () {

    const removeItem = (values, value) => {
        const index = values.indexOf(value);

        if (index > -1) {
            values.splice(index, 1);

            return true;
        }

        return false;
    }
}();
