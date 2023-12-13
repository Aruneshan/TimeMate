document.addEventListener('DOMContentLoaded', function () {
    var passwordInput = document.getElementById('Input_Password');
    var passwordStrengthIndicator = document.getElementById('password-strength-indicator');

    passwordInput.addEventListener('input', function () {
        var password = passwordInput.value;
        var strength = checkPasswordStrength(password);

        if (strength === 0) {
            passwordStrengthIndicator.innerHTML = 'Weak';
            passwordStrengthIndicator.style.color = 'red';
        } else if (strength === 1) {
            passwordStrengthIndicator.innerHTML = 'Normal';
            passwordStrengthIndicator.style.color = 'orange';
        } else if (strength >= 2) {
            passwordStrengthIndicator.innerHTML = 'Strong';
            passwordStrengthIndicator.style.color = 'green';
        }
    });

    function checkPasswordStrength(password) {
        var strength = 0;

        // Check length
        if (password.length >= 8)
            strength += 1;

        // Check complexity (use regular expressions)
        if (/[a-z]/.test(password) && /[A-Z]/.test(password))
            strength += 1;

        if (/[0-9]/.test(password))
            strength += 1;

        if (/[!@#$%^&*]/.test(password))
            strength += 1;

        return strength;
    }
});
