import React from 'react'
import ReactDOM from 'react-dom/client'
import Login from './component/Login.jsx'
import {
  createBrowserRouter,RouterProvider,
} from "react-router-dom";
import Register from './component/Register.jsx';
import Homepage from './component/Hompage.jsx';
import AddShop from './component/AddShop.jsx';
import Shop from './component/Shop.jsx';
import Review from './component/Review.jsx';

const router = createBrowserRouter([
  {
    path:"/login",
    element:<Login/>
  },
  {
    path:"/register",
    element:<Register/>
  },
  {
    path:"/homepage",
    element:<Homepage/>
  },
  {
    path:"/addshop",
    element:<AddShop/>
  }
  ,
  {
    path:"/shop",
    element:<Shop/>
  }
  ,
  {
    path:"/review",
    element:<Review/>
  }
])


ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
      <RouterProvider router={router}/>
  </React.StrictMode>,
  
)
