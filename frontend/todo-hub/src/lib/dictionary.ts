export type Lang = "en" | "de";

export const translations: Record<
  Lang,
  {
    newTodo: string;
    editTodo: string;
    titlePlaceholder: string;
    descriptionPlaceholder: string;
    cancel: string;
    create: string;
    modify: string;
    createErrorPrefix?: string;
    dashboard: string;
    admin: string;
    profile: string;
    home: string;
    login: string;
    name: string;
    email: string;
    password: string;
    confirmPassword: string;
    logout: string;
    signUp: string;
    alreadyHaveAccount: string;
    dontHaveAccount: string;
    register: string;
    createTodo: string;
    created: string;
    accountProfile: string;
    sortNewest: string;
    sortOldest: string;
    settings: string;
  }
> = {
  en: {
    newTodo: "New Todo",
    editTodo: "Edit Todo",
    titlePlaceholder: "Title",
    descriptionPlaceholder: "Description",
    cancel: "Cancel",
    create: "Create",
    modify: "Modify",
    createErrorPrefix: "Error:",
    dashboard: "Dashboard",
    admin: "Admin",
    profile: "Profile",
    home: "Home",
    login: "Login",
    name: "Name",
    email: "Email",
    password: "Password",
    confirmPassword: "Confirm Password",
    logout: "Logout",
    signUp: "Sign Up",
    alreadyHaveAccount: "Already have an account? ",
    dontHaveAccount: "Don't have an account? ",
    register: "Register",
    createTodo: "Create Todo",
    created: "created",
    accountProfile: "Account Profile",
    sortNewest: "Sort by Newest",
    sortOldest: "Sort by Oldest",
    settings: "Settings",
  },
  de: {
    newTodo: "Neues Todo",
    editTodo: "Todo bearbeiten",
    titlePlaceholder: "Titel",
    descriptionPlaceholder: "Beschreibung",
    cancel: "Abbrechen",
    create: "Erstellen",
    modify: "Ändern",
    createErrorPrefix: "Fehler:",
    dashboard: "Tafel",
    admin: "Admin",
    profile: "Profil",
    home: "Startseite",
    login: "Anmelden",
    name: "Name",
    email: "E-Mail",
    password: "Passwort",
    confirmPassword: "Passwort bestätigen",
    logout: "Abmelden",
    signUp: "Registrieren",
    alreadyHaveAccount: "Haben Sie bereits ein Konto? ",
    dontHaveAccount: "Sie haben kein Konto? ",
    register: "Registrieren",
    createTodo: "Todo erstellen",
    created: "erstellt",
    accountProfile: "Kontoprofil",
    sortNewest: "Nach Neueste sortieren",
    sortOldest: "Nach Älteste sortieren",
    settings: "Einstellungen",
  },
};