import { useState, useEffect } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';
import {
  Link,
} from "react-router-dom";


function AddShop() {
  const navigateTo = useNavigate();
  const [shopname, setShopName] = useState('');
  const [addressnumber, setAddressNumber] = useState('');
  const [detail, setDetail] = useState('');
  const [district, setDistrict] = useState('');
  const [province, setProvince] = useState('');
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

  const handleSubmit = async (e) => {
    e.preventDefault();

    try {
      const formData = new FormData();
      formData.append('ShopName', shopname);
      formData.append('AddressNumber', addressnumber);
      formData.append('Detail', detail);
      formData.append('District', district);
      formData.append('Province', province);

      

      const token = localStorage.getItem('token');
      const response = await axios.post('https://localhost:7199/Shop', formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
          'Authorization': `${token}` 
        }
      });

      setSuccessMessage('สร้างร้านค้าสำเร็จ!');
      console.log(response.data);
      setTimeout(() => {
        navigateTo('/homepage');
      }, 3000);
      
    } catch (error) {
      if (error.response && error.response.data && error.response.data.errors) {
        setError(error.response.data.errors);
      } else {
        setError('ผู้ใช้งานมีร้านค้าอยู่แล้ว');
      }
    }
  };

  return (
    <div>
      <Link to="/homepage" className='backlogin-button'>Homepage</Link>
      <h1>AddShop</h1>
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
          <label>ShopName:</label>
          <input
            type="text"
            value={shopname}
            onChange={(e) => setShopName(e.target.value)}
            required
          />
        </div>
        <div>
          <label>AddressNumber:</label>
          <input
            type="text"
            value={addressnumber}
            onChange={(e) => setAddressNumber(e.target.value)}
            required
          />
        </div>
        <div>
          <label>Detail:</label>
          <input
            type="text"
            value={detail}
            onChange={(e) => setDetail(e.target.value)}
            required
          />
        </div>
        <div>
          <label>District:</label>
            <input
            type="text"
            value={district}
            onChange={(e) => setDistrict(e.target.value)}
            required
          />
        </div>
        <div>
          <label>Province:</label>
          <input
            type="text"
            value={province}
            onChange={(e) => setProvince(e.target.value)}
            required
          />
        </div>
        <button type="submit">AddShop</button>
      </form>
      </div>
    </div>
  );
}

export default AddShop;
