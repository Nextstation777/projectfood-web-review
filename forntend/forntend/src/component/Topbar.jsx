import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import '../style/Topbar.css';

const Topbar = () => {
  const [username, setUsername] = useState('');
  const [userImage, setUserImage] = useState(null);
  const navigateTo = useNavigate();

  useEffect(() => {
    const token = localStorage.getItem('token');
    if (token) {
      const parsedToken = parseBearerToken(token);
      setUsername(parsedToken.username);
      fetchUserImage(parsedToken.username2);
    }
  }, []);

  const parseBearerToken = (token) => {
    const [, tokenBody] = token.split('Bearer '); 
    const tokenData = JSON.parse(atob(tokenBody.split('.')[1])); 
    const username = tokenData.Name;
    const username2 = tokenData.nameid;
    return { username ,username2};
  };

  const fetchUserImage = async (username2) => {
    try {
      const response = await axios.get(`https://localhost:7199/User/${username2}`);
      setUserImage(response.data.profilePic);
    } catch (error) {
      console.error('Error fetching user image:', error);
    }
  };

  const handleLogout = () => {
    localStorage.removeItem('token');
    navigateTo('/login');
  };

  return (
    <div className="topbar-container">
      <div className="left-section">
        <h1 className="title4">Thiao Nai</h1>
      </div>
      <div className="center-section">
        <input type="text" placeholder="ค้นหา..." />
      </div>
      <div className="right-section">
        <div className="user-info">
          {userImage ? (
            <img src={userImage} alt="User" />
          ) : (
            <span>Loading...</span>
          )}
          <span className="user-name">{username}</span>
          <button className="logout-button" onClick={handleLogout}>Logout</button>
        </div>
      </div>
    </div>
  );
}

export default Topbar;
