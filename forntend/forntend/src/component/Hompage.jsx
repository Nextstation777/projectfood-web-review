import { useState, useEffect } from 'react';
import axios from 'axios';
import '../style/Homepage.css';
import Topbar from './Topbar';
import Navbar from './Navbar';

const Homepage = () => {
  const [recomments, setRecomments] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchRecomments = async () => {
      try {
        const response = await axios.get('https://localhost:7199/Recomment');
        setRecomments(response.data);
        setIsLoading(false);
      } catch (error) {
        setError('Error fetching recomments');
        setIsLoading(false);
      }
    };

    fetchRecomments();
  }, []);

  return (
    <div>
      <div className="topbar-home"><Topbar /></div>
      <div className="navbar-home"><Navbar /></div>
      <div> <h1 className="title5">Recommend</h1></div>
      {isLoading ? (
        <p>Loading...</p>
      ) : error ? (
        <p>{error}</p>
      ) : (
        
        <div className="recomments-container">
          {recomments.map((recomment) => (
            <div className="card" key={recomment.postId}>
              <div className="top-card">
                {recomment.coverPhotoUrl && <img src={recomment.coverPhotoUrl} alt="Recomment" />}
              </div>
              <div className="bottom-card">
                <h2>{recomment.shopName}</h2>
                <p>Recommend: {recomment.recom}</p>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default Homepage;
