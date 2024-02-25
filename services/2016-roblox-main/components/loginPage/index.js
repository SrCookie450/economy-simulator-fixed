// based on https://web.archive.org/web/20160126164519/https://www.roblox.com/NewLogin
// todo: password reset link, once page is added

import React from "react";
import LoginArea from "./components/loginArea";
import SignUpArea from "./components/signUpArea";

const Login = () => {
    return <div className='container'>
        <div className='row'>
            <div className='col-12 col-lg-6 divider-right'>
                <LoginArea></LoginArea>
            </div>
            <div className='col-12 col-lg-6'>
                <SignUpArea></SignUpArea>
            </div>
        </div>
        <div className='mt-4 divider-bottom'></div>
    </div>
}

export default Login;