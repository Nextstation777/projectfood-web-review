import { useState, useEffect } from 'react';
function Review() {
    const [count, setCount] = useState(0);

    // ใช้ useEffect เพื่อทำงานหลังจากการ render ครั้งแรกและหลังจากทุกครั้งที่ count เปลี่ยนแปลง
    useEffect(() => {
      // รันตัว effect
  
      // แสดงข้อความใน console เมื่อเกิดการเปลี่ยนแปลงค่าของ count
      console.log(`You clicked ${count} times`);
  
      // ตัว effect สามารถ return function เพื่อทำความสะอาดหรือ unsubscribe จากการทำงานหลังจากนั้น
      return () => {
        // การทำความสะอาดหรือ unsubscribe จะถูกทำทุกครั้งก่อนที่ effect ถัดไปจะถูกเรียกใช้งาน
        console.log('Clean up');
      };
    }, [count]); // ตัว dependencies เป็น optional และถ้าไม่ได้ระบุ จะทำให้ effect รันทุกครั้งที่ render
  
    return (
      <div>
        <p>You clicked {count} times</p>
        {/* ปุ่มที่ใช้สำหรับเพิ่มค่า count */}
        <button onClick={() => setCount(count + 1)}>
          Click me
        </button>
      </div>
    );
}

export default Review;