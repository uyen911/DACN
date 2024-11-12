const container = document.querySelector('.containers')
const btnSignIn = document.querySelector('.btnSign-in')
const btnSignUp = document.querySelector('.btnSign-up')

btnSignIn.addEventListener('click', () => {
    container.classList.add('active')
})

btnSignUp.addEventListener('click', () => {
    container.classList.remove('active')
})
// login.js
$(document).ready(function () {
    $('#forgotPasswordButton').click(function () {
        $('#loginForm').hide();
        $('#forgotPasswordForm').show();
    });
});
