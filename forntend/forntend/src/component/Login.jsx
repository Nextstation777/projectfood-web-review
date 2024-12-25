import { useState } from 'react';
import yyy from 'axios';
import { useNavigate } from 'react-router-dom';
import '../style/Login.css';
import {
  Link,
} from "react-router-dom";


function Login() {
  const navigateTo = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState(null);
  
  

  const handleSubmit = async (event) => {
    event.preventDefault();

    try {
      const formData = new FormData();
      formData.append('Email', email);
      formData.append('Password', password);

      const response = await yyy.post('https://localhost:7199/Login', formData, {
        headers: {
          'Content-Type': 'multipart/form-data'
        }
      });
      if(response.status)    
      {
        const receivedToken = response.data.token;
        localStorage.setItem('token', 'Bearer ' + receivedToken);
        navigateTo('/homepage');
      }else{
        setError('Not found');
      }
      
    } catch (error) {
      if (error.response && error.response.data && error.response.data.errors) {
        setError(error.response.data.errors);
      } else {
        setError('ชื่อผู้ใช้งานหรือรหัสผ่านไม่ถูกต้อง');
      }
    }
  };

  return (
    <div className="login-container">
      <h1>Login</h1>
      {error && (
        <div style={{ color: 'red', fontWeight: 'bold' }}>
          <strong>Error:</strong>
          <span>{error}</span>
        </div>
      )}
      <form onSubmit={handleSubmit}>
        <div>
          <label>Email or UserName:</label>
          <input
            type="text"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
        </div>
        <div>
          <label>Password:</label>
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </div>
        <button className='login-button' type="submit">Login</button> 
        <Link to="/Register" className='register-button'>Register</Link>
      </form>
    </div>
  );
}

export default Login;
