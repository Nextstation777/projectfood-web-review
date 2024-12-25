import { useState, useEffect } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';
import '../style/Register.css';
import {
  Link,
} from "react-router-dom";

function Register() {
  const navigateTo = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [username, setUsername] = useState('');
  const [name, setName] = useState('');
  const [profilepic, setProfilePic] = useState(null);
  const [error, setError] = useState(null);
  const [successMessage, setSuccessMessage] = useState('');

  useEffect(() => {
    const timer = setTimeout(() => {
      setSuccessMessage('');
    }, 3000);
    return () => clearTimeout(timer);
  }, [successMessage]);

  useEffect(() => {
    if (error) {
      const timer = setTimeout(() => {
        setError(null);
      }, 3000);
      return () => clearTimeout(timer);
    }
  }, [error]);

  const handleSubmit = async (event) => {
    event.preventDefault();
    if (password !== confirmPassword) {
      setError('รหัสผ่านไม่ตรงกัน');
      return;
    }

    try {
      const formData = new FormData();
      formData.append('Email', email);
      formData.append('Password', password);
      formData.append('UserName', username);
      formData.append('Name', name);
      formData.append('Profilepic', profilepic);

      const response = await axios.post('https://localhost:7199/User', formData, {
        headers: {
          'Content-Type': 'multipart/form-data'
        }
      });

      setSuccessMessage('สมัครสมาชิกสำเร็จ!');
      console.log('Registration successful!', response.data);
      setTimeout(() => {
        navigateTo('/login');
      }, 3000);
      
    } catch (error) {
      if (error.response && error.response.data && error.response.data.errors) {
        setError(error.response.data.errors);
      } else {
        setError('มีผู้ใช้งาน UserName หรือ Email ซ้ำ');
      }
    }
  };

  return (
    <div>
      <Link to="/Login" className='backlogin-button'>Login</Link>
      <h1>Register</h1>
      <div className="register-container">
      {error && (
        <div style={{ color: 'red', fontWeight: 'bold' }}>
          <strong>Error:</strong>
          <span>{error}</span>
        </div>
      )}
      {successMessage && (
        <div style={{ color: 'green', fontWeight: 'bold' }}>
          <strong>Success:</strong>
          <span>{successMessage}</span>
        </div>
      )}
      <form onSubmit={handleSubmit}>
        <div>
          <label>UserName:</label>
          <input
            type="text"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            required
          />
        </div>
        <div>
          <label>Email:</label>
          <input
            type="email"
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
        <div>
          <label>Confirm Password:</label>
            <input
            type="password"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            required
          />
        </div>
        <div>
          <label>Name:</label>
          <input
            type="text"
            value={name}
            onChange={(e) => setName(e.target.value)}
            required
          />
        </div>
        <div>
          <label>ProfilePic:</label>
          <input
            type="file"
            onChange={(e) => setProfilePic(e.target.files[0])}
            required
          />
        </div>
        <button type="submit">Register</button>
      </form>
      </div>
    </div>
  );
}

export default Register;
