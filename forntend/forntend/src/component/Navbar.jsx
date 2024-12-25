import { Link, useLocation } from 'react-router-dom'; 
import '../style/Navbar.css';
import { useNavigate } from 'react-router-dom';
import { useEffect, useState } from 'react';
import axios from 'axios';

const Navbar = () => {
  const location = useLocation();
  const navigateTo = useNavigate(); 
  const [userid, setUserId] = useState('');
  const [hasShop, setHasShop] = useState(false);

  useEffect(() => {
    const token = localStorage.getItem('token');
    if (token) {
      const parsedToken = parseBearerToken(token);
      setUserId(parsedToken.userid);

      const checkHasShop = async () => {
          const response2 = await axios.get(`https://localhost:7199/Shop/${userid}`);
          if (response2.data.length > 0) {
            setHasShop(false);
          } else {
            setHasShop(true);
          }      
      };
  
      checkHasShop();
    }
  }, [userid]);
  
  const parseBearerToken = (token) => {
    const [, tokenBody] = token.split('Bearer '); 
    const tokenData = JSON.parse(atob(tokenBody.split('.')[1])); 
    const userid = tokenData.UserId;
    return { userid };
  };

  const handleAddShop = () => {
    navigateTo('/addshop');
  }

  return (
    <div className="navbar-container">
      <nav className="navbar">
        <ul className="nav-menu">
          <li className={`nav-item ${location.pathname === '/homepage' ? 'active' : ''}`}>
            <Link to="/homepage">หน้าหลัก</Link>
          </li>
          <li className={`nav-item ${location.pathname === '/shop' ? 'active' : ''}`}>
            <Link to="/shop">ร้านค้า</Link>
          </li>
          <li className={`nav-item ${location.pathname === '/review' ? 'active' : ''}`}>
            <Link to="/review">รีวิว</Link>
          </li>
        </ul>
      </nav>
      {!hasShop && (
        <div className='add-button'>
          <button onClick={handleAddShop}>เพิ่มร้านค้า</button>
        </div>
      )}
    </div>
  );
}

export default Navbar;
