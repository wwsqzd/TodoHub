'use client'
import { useState, useEffect } from "react"

export default function Daschboard() 
{
    const [todos, setTodos] = useState([]);

    // useEffect(() => {
    //     const getTodos = async () => {
            
    //     }
    // }, [])

    return (
        <div className="w-full h-screen flex justify-center">
            <div className="flex flex-col items-center gap-5">
                <h1 className="text-4xl font-bold">Welcome to your Dashboard!</h1>
                <p>Lets create a first to-do!</p>
                {/* <button className="w-44 bg-blue-400 cursor-pointer">Create T</button> */}
            </div>
            
        </div>
    )
}