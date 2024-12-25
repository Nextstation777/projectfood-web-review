import { useState, useEffect } from 'react';
import axios from 'axios';
import '../style/Shop.css';
import Topbar from './Topbar';
import Navbar from './Navbar';

const Post = () => {
  const [post, setPost] = useState([]);

  useEffect(() => {
    const fetchPost = async () => {
      try {
        const response = await axios.get(`https://localhost:7199/Post/${post.postid}`);
        setPost(response.data);
      } catch (error) {
        console.error('Error fetching recomments:', error);
      }
    };

    fetchPost();
  },);

  return (
    <div>
      <div className="topbar-home" style={{ zIndex: 2 }}><Topbar /></div>
      <div className="navbar-home" style={{ zIndex: 2 }}><Navbar /></div>
      <div className="shop-content" style={{ zIndex: 1 }}>
        <h1 className="title3">Shop</h1>
        <div className="shops-container">
          {post.map((post) => (
            <div className="card" key={post.postId}>
              <div className="top-card">
                {[post].coverPhotoUrl && <img src={post.coverPhotoUrl} alt="shop" />}
              </div>
              <div className="bottom-card">
                <h2>{post.shopName}</h2>
                <p>{post.postScore}</p>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

export default Post;
