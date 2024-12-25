import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import axios from 'axios';
import '../style/Shop.css';
import Topbar from './Topbar';
import Navbar from './Navbar';

const Shop = () => {
  const [shops, setShops] = useState([]);

  useEffect(() => {
    const fetchShops = async () => {
      try {
        const response = await axios.get('https://localhost:7199/Posts');
        setShops(response.data);
      } catch (error) {
        console.error('Error fetching shops:', error);
      }
    };

    fetchShops();
  }, []);

  return (
    <div>
      <div className="topbar-home" style={{ zIndex: 2 }}><Topbar /></div>
      <div className="navbar-home" style={{ zIndex: 2 }}><Navbar /></div>
      <div className="shop-content" style={{ zIndex: 1 }}>
        <h1 className="title3">Shop</h1>
        <div className="shops-container">
          {shops.map((shop) => (
            <Link to={`/shop/${shop.postId}`} key={shop.postId} className="card">
              <div className="top-card">
                {shop.coverPhotoUrl && <img src={shop.coverPhotoUrl} alt="shop" />}
              </div>
              <div className="bottom-card">
                <h2>{shop.shopName}</h2>
                <p>{shop.postScore}</p>
              </div>
            </Link>
          ))}
        </div>
      </div>
    </div>
  );
}

export default Shop;
