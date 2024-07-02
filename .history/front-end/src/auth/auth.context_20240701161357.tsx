import { ReactNode, createContext, useReducer, useCallback, useEffect } from 'react';
import {
    IAuthContext,
    IAuthContextAction,
    IAuthContextActionTypes,
    IAuthContextState,
    ILoginResponseDto,
} from '../types/auth.types';
import { getSession, setSession } from './auth.utils';
import axiosInstance from '../utils/axiosInstance';
import toast from 'react-hot-toast';
import { useNavigate } from 'react-router-dom';
import {
    LOGIN_URL,
    ME_URL,
    PATH_AFTER_LOGIN,
    PATH_AFTER_LOGOUT,
    PATH_AFTER_REGISTER,
    REGISTER_URL,
} from '../utils/globalConfig';

// We need a reducer function for useReducer hook
const authReducer = (state: IAuthContextState, action: IAuthContextAction) => {
if (action.type === IAuthContextActionTypes.LOGIN) {
    return {
    ...state,
    isAuthenticated: true,
    isAuthLoading: false,
    user: action.payload,
    };
}
if (action.type === IAuthContextActionTypes.LOGOUT) {
    return {
    ...state,
    isAuthenticated: false,
    isAuthLoading: false,
    user: undefined,
    };
}
return state;
};

// We need an initial state object for useReducer hook
const initialAuthState: IAuthContextState = {
isAuthenticated: false,
isAuthLoading: true,
user: undefined,
};

// We create our context here and export it
export const AuthContext = createContext<IAuthContext | null>(null);

// We need an interface for our context props
interface IProps {
children: ReactNode;
}

// We create a component to manage all auth functionalities and export it and use it
const AuthContextProvider = ({children} : IProps) => {
    const [state, dispatch] = useReducer(authReducer, initialAuthState);
    const navigate = useNavigate();

    //Initialize method
    const initialAuthContext = useCallback(async() => {
        try {
            const token = getSession();
            if(token){
                 // validate accessToken by calling backend
                const response = await axiosInstance.post<ILoginResponseDto>(ME_URL, {
                    token,
                });
                // In response, we receive jwt token and user data
                const {newToken, userInfo} = response.data;
                setSession(newToken);
                dispatch({
                    type: IAuthContextActionTypes.LOGIN,
                    payload:userInfo
                });
            }else{
                setSession(null);
                dispatch({
                    type:IAuthContextActionTypes.LOGOUT
                });
            }
            
        } catch (error) {
            setSession(null);
            dispatch({
                type: IAuthContextActionTypes.LOGOUT,
            });
        }
    }, []);
    // In start of Application, We call initializeAuthContext to be sure about authentication status
    useEffect(() => {
        console.log('AuthContext Initialization start');
        initialAuthContext()
        .then(() => console.log('initializeAuthContext was successfull'))
        .catch((error) => console.log(error));
    }, []);
    // Register Method
    const register = useCallback(
        async(firstName:string, lastName:string, userName:string, email:string, password:string, address:string) => {
            const response = await 
        }
    )
}