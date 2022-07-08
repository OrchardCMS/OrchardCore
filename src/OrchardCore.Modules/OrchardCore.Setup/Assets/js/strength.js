(function ($) {

    $.fn.strength = function (options) {

        var settings = $.extend({
            minLength: 8,
            upperCase: false,
            lowerCase: false,
            numbers: false,
            specialchars: false,
            target: '',
            style: ''
        }, options);


        var capitalletters = 0;
        var lowerletters = 0;
        var numbers = 0;
        var specialchars = 0;

        var upperCase = new RegExp('[A-Z]');
        var lowerCase = new RegExp('[a-z]');
        var number = new RegExp('[0-9]');
        var specialchar = new RegExp('[^A-Za-z0-9]');

        var valid = false;

        createProgressBar(0, '');

        function getPercentage(a, b) {
            return ((b / a) * 100).toFixed(0);
        }

        function getLevel(value) {

            if (value >= 100) {
                return "bg-success";
            }

            if (value >= 50) {
                return "bg-warning";
            }

            if (value == 0) {
                return ''; // grayed
            }

            return "bg-danger";
        }

        function checkStrength(value) {

            minLength = value.length >= settings.minLength ? 1 : 0;
            capitalletters = !settings.upperCase || value.match(upperCase) ? 1 : 0;
            lowerletters = !settings.lowerCase || value.match(lowerCase) ? 1 : 0;
            numbers = !settings.numbers || value.match(number) ? 1 : 0;
            specialchars = !settings.specialchars || value.match(specialchar) ? 1 : 0;

            var total = minLength + capitalletters + lowerletters + numbers + specialchars;
            var percentage = getPercentage(5, total);

            valid = percentage >= 100;

            createProgressBar(percentage, getLevel(percentage));
        }

        function createProgressBar(percentage, level) {
            var el = $('<div class="progress" value="' + percentage + '" style="' + settings.style + '" max="100" aria-describedby=""><div class="progress-bar ' + level + '" role="progress-bar" style="width: ' + percentage + '%;"></div></div>');
            var target = $(settings.target);
            target.empty();
            target.append(el);
        }

        this.bind('keyup keydown', function (event) {
            checkStrength($(this).val());
        });
        this.bind('drop', function (event) {
            checkStrength(event.originalEvent.dataTransfer.getData("text"));
        });

        this.parents('form').on('submit', function () {
            if (!valid) {
                event.preventDefault();
            }
        });
    };
}(jQuery));
    